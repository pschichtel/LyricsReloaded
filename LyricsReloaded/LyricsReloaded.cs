using System;
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
        private string name = "LyricsReloaded";
        private string pluginDirectory = ".\\Plugins";
        private LyricsLoader loader;
        private Dictionary<String, LyricsProvider> providers = new Dictionary<string, LyricsProvider>();
        private Dictionary<string, Filter> filters = new Dictionary<string, Filter>();

        // Called from MusicBee
        public PluginInfo Initialise(IntPtr apiPtr)
        {
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

            Assembly asm = Assembly.GetAssembly(this.GetType());
            this.name = asm.GetName().Name;
            this.pluginDirectory = Path.GetDirectoryName(asm.Location);
            this.logger = new Logger(this.pluginDirectory + Path.DirectorySeparatorChar + this.name + ".log");

            return this.info;
        }

        public void ReceiveNotification(String source, NotificationType type)
        {
            this.logger.debug("Received a notification of type %s", type);
            switch (type)
            {
                case NotificationType.PluginStartup:
                    this.init();

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
            this.logger.info("Closing ...");
            this.providers.Clear();
            this.filters.Clear();
            this.loader = null;
            this.initialized = false;

            if (reason == PluginCloseReason.MusicBeeClosing)
            {
                this.logger.close();
            }
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

            DirectoryInfo di = new DirectoryInfo(this.pluginDirectory + Path.DirectorySeparatorChar + this.name);
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
            LyricsProvider provider;
            try
            {
                provider = this.providers[providerName];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }

            String url = provider.constructUrl(artist, title, album, preferSynced);

            Console.WriteLine("URL: " + url);

            LyricsResponse response = this.loader.loadContent(url, "USER_AGENT");

            return provider.processContent(response.getContent(), response.getEncoding());
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

            api.Setting_GetWebProxy = delegate() { return ""; };

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(api));
            Marshal.StructureToPtr(api, ptr, true);

            return ptr;
        }
    }
}
