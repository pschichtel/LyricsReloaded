/*
    Copyright 2013 Phillip Schichtel

    This file is part of LyricsReloaded.

    LyricsReloaded is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    LyricsReloaded is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with LyricsReloaded. If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;
using CubeIsland.LyricsReloaded;
using CubeIsland.LyricsReloaded.Filters;
using System.Reflection;
using YamlDotNet.RepresentationModel;
using System.Resources;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private static readonly YamlScalarNode NODE_NAME = new YamlScalarNode("name");
        private static readonly YamlScalarNode NODE_URL = new YamlScalarNode("url");
        private static readonly YamlScalarNode NODE_EXPRESSIONS = new YamlScalarNode("expressions");
        private static readonly YamlScalarNode NODE_FILTERS = new YamlScalarNode("filters");

        private MusicBeeApiInterface musicBee;
        private PluginInfo info = new PluginInfo();
        private Logger logger;
        private bool initialized = false;
        private string name;
        private string pluginDirectory;
        private LyricsLoader loader;
        private Dictionary<String, LyricsProvider> providers = new Dictionary<string, LyricsProvider>();
        private Dictionary<string, Filter> filters = new Dictionary<string, Filter>();

        // Called from MusicBee
        public PluginInfo Initialise(IntPtr apiPtr)
        {
            //MessageBox.Show("Initialised(" + apiPtr + ")");
            this.musicBee = new MusicBeeApiInterface();
            this.musicBee.Initialise(apiPtr);

            this.info.PluginInfoVersion = PluginInfoVersion;
            this.info.Name = "Lyrics Reloaded!";
            this.info.Description = "Lyrics loading done properly!";
            this.info.Author = "Phillip Schichtel <Quick_Wango>";
            this.info.TargetApplication = "MusicBee";
            this.info.Type = PluginType.LyricsRetrieval;
            this.info.VersionMajor = 1;
            this.info.VersionMinor = 0;
            this.info.Revision = 1;
            this.info.MinInterfaceVersion = 20;
            this.info.MinApiRevision = 25;
            this.info.ReceiveNotifications = ReceiveNotificationFlags.StartupOnly;
            this.info.ConfigurationPanelHeight = 0;

            try
            {
                Assembly asm = Assembly.GetAssembly(this.GetType());
                this.name = asm.GetName().Name;
                this.pluginDirectory = Path.Combine(this.musicBee.Setting_GetPersistentStoragePath(), this.name);
                Directory.CreateDirectory(this.pluginDirectory);
                this.logger = new Logger(Path.Combine(this.pluginDirectory, this.name + ".log"));
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred during plugin startup: " + e.Message);
                throw e;
            }

            try
            {
                this.init();
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred during plugin startup, send this file to the developer:\n\n" + this.logger.getFileInfo().FullName);
                this.logger.error(e.Message);
                throw e;
            }

            return this.info;
        }

        public void ReceiveNotification(String source, NotificationType type)
        {
            //MessageBox.Show("ReceiveNotification(" + source + ", " + type + ")");
            this.logger.debug("Received a notification of type {0}", type);
            switch (type)
            {
                case NotificationType.PluginStartup:
                    String proxySetting = this.musicBee.Setting_GetWebProxy();
                    if (!string.IsNullOrEmpty(proxySetting))
                    {
                        this.logger.debug("Proxy setting found");
                        string[] raw = proxySetting.Split(Convert.ToChar(0));
                        WebProxy proxy = new WebProxy(raw[0]);
                        if (raw.Length >= 3)
                        {
                            this.logger.debug("Proxy credentials found");
                            proxy.Credentials = new NetworkCredential(raw[1], raw[2]);
                        }
                        this.loader.setProxy(proxy);
                    }

                    break;
            }
        }

        public void Close(PluginCloseReason reason)
        {
            //MessageBox.Show("Close(" + reason + ")");
            this.logger.info("Closing ...");
            this.providers.Clear();
            this.filters.Clear();
            this.loader = null;
            this.logger.close();
            this.logger = null;
            this.initialized = false;
        }

        public void init()
        {
            if (!this.initialized)
            {
                this.initialized = true;
                this.loader = new LyricsLoader(this, 20000);
                this.initFilters();
                this.loadProviders();
                this.logger.info("Plugin initialized!");
            }
        }

        private void initFilters()
        {
            Type filterType = typeof(Filter);
            foreach (Type type in Assembly.GetAssembly(this.GetType()).GetTypes())
            {
                if (filterType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                {
                    try
                    {
                        Filter filter = (Filter)Activator.CreateInstance(type);
                        this.filters.Add(filter.getName(), filter);
                    }
                    catch
                    {}
                }
            }
        }

        private void loadProviders()
        {
            this.loadBuildInProviders();

            DirectoryInfo di = new DirectoryInfo(Path.Combine(this.pluginDirectory, "providers"));
            if (!di.Exists)
            {
                di.Create();
            }

            foreach (FileInfo fi in di.GetFiles("*.yml", SearchOption.TopDirectoryOnly))
            {
                using (StreamReader reader = fi.OpenText())
                {
                    this.loadProvider(reader);
                }
            }
        }

        private void loadBuildInProviders()
        {
            this.loadProviderFromResource(Properties.Resources.songlyrics_com);
            this.loadProviderFromResource(Properties.Resources.metrolyrics_com);
            this.loadProviderFromResource(Properties.Resources.letras_mus_br);
            this.loadProviderFromResource(Properties.Resources.teksty_org);
            this.loadProviderFromResource(Properties.Resources.tekstowo_pl);
            this.loadProviderFromResource(Properties.Resources.azlyrics_com);
            this.loadProviderFromResource(Properties.Resources.plyrics_com);
            this.loadProviderFromResource(Properties.Resources.urbanlyrics_com);
            this.loadProviderFromResource(Properties.Resources.rapgenius_com);
            this.loadProviderFromResource(Properties.Resources.oldielyrics_com);
        }

        private void loadProviderFromResource(byte[] resourceData)
        {
            this.loadProvider(new StreamReader(new MemoryStream(resourceData), Encoding.UTF8));
        }

        public void loadProvider(TextReader configReader)
        {
            YamlStream yaml = new YamlStream();
            yaml.Load(configReader);

            YamlMappingNode root = (YamlMappingNode)yaml.Documents[0].RootNode;

            YamlNode node = root.Children[NODE_NAME];
            if (!(node is YamlScalarNode))
            {
                throw new IOException("Invalid configuration");
            }
            string name = ((YamlScalarNode)node).Value;

            node = root.Children[NODE_URL];
            if (!(node is YamlScalarNode))
            {
                throw new IOException("Invalid configuration");
            }
            string url = ((YamlScalarNode)node).Value;

            node = root.Children[NODE_EXPRESSIONS];
            LinkedList<Expression> expressions = new LinkedList<Expression>();
            if (node is YamlScalarNode)
            {
                expressions.AddLast(new Expression(((YamlScalarNode)node).Value));
            }
            else if (node is YamlSequenceNode)
            {
                YamlSequenceNode seq = (YamlSequenceNode)node;
                foreach (YamlNode entry in seq.Children)
                {
                    if (entry is YamlScalarNode)
                    {
                        expressions.AddLast(new Expression(((YamlScalarNode)entry).Value));
                    }
                }
            }
            else
            {
                throw new IOException("Invalid configuration");
            }

            node = root.Children[NODE_FILTERS];
            LinkedList<Filter> filters = new LinkedList<Filter>();
            Filter tmp;
            if (node is YamlScalarNode)
            {
                tmp = this.getFilter(((YamlScalarNode)node).Value);
                if (tmp != null)
                {
                    filters.AddLast(tmp);
                }
            }
            else if (node is YamlSequenceNode)
            {
                YamlSequenceNode seq = (YamlSequenceNode)node;
                foreach (YamlNode entry in seq.Children)
                {
                    if (entry is YamlScalarNode)
                    {
                        tmp = this.getFilter(((YamlScalarNode)entry).Value);
                        if (tmp != null)
                        {
                            filters.AddLast(tmp);
                        }
                    }
                }
            }
            else
            {
                throw new IOException("Invalid configuration");
            }

            LyricsProvider provider = new LyricsProvider(this, name, url, expressions, filters);
            this.logger.info("Provider loaded: " + provider.getName());

            lock (this.providers)
            {
                if (this.providers.ContainsKey(provider.getName()))
                {
                    this.logger.info("The provider %s does already exist and will be replaced.", provider.getName());
                    this.providers.Remove(provider.getName());
                }
                this.providers.Add(provider.getName(), provider);
            }
        }

        public string getName()
        {
            return this.name;
        }

        public MusicBeeApiInterface getMusicBee()
        {
            return this.musicBee;
        }

        public Logger getLogger()
        {
            return this.logger;
        }

        public Filter getFilter(string name)
        {
            name = name.ToLower();
            if (filters.ContainsKey(name))
            {
                return filters[name];
            }
            return null;
        }

        public String[] GetProviders()
        {
            String[] providers = new String[this.providers.Count];
            this.providers.Keys.CopyTo(providers, 0);
            return providers;
        }

        public String RetrieveLyrics(String source, String artist, String title, String album, bool preferSynced, String providerName)
        {
            this.logger.debug("Lyrics request: {0} - {1} - {2} - {3} - {4} - {5}", source, artist, title, album, (preferSynced ? "synced" : "unsynced"), providerName);
            LyricsProvider provider;
            try
            {
                provider = this.providers[providerName];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }

            string url = provider.constructUrl(artist, title, album, preferSynced);

            this.logger.debug("{0} constructed this URL: {1}", provider.getName(), url);

            LyricsResponse response = this.loader.loadContent(url, "USER_AGENT");

            String content = provider.processContent(response.getContent(), response.getEncoding());

            if (String.IsNullOrWhiteSpace(content))
            {
                this.logger.debug("no lyrics found");
                return null;
            }

            this.logger.debug("lyrics found");

            return content;
        }

        #region "MusicBee implementations"
        public bool Configure(IntPtr panelHandle)
        {
            return true;
        }

        public void SaveSettings()
        {}

        public void Uninstall()
        {}

        #endregion

        // used for testing only
        public static IntPtr mockApi()
        {
            MusicBeeApiInterface api = new Plugin.MusicBeeApiInterface();

            api.Setting_GetWebProxy = delegate() {
                return "";
            };

            api.Setting_GetPersistentStoragePath = delegate() {
                return Path.GetDirectoryName(Assembly.GetAssembly(typeof(LyricsLoader)).Location);
            };

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(api));
            Marshal.StructureToPtr(api, ptr, true);

            return ptr;
        }
    }
}
