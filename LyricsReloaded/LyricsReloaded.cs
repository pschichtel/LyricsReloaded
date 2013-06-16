using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;
using CubeIsland.LyricsReloaded;
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

        public static MusicBeeApiInterface API;
        private bool initialized = false;
        private string pluginDirectory = ".\\Plugins";
        private LyricsLoader loader;
        private Dictionary<String, LyricsReader> readers = new Dictionary<string, LyricsReader>();
        private Dictionary<string, Filter> filters = new Dictionary<string, Filter>();

        // Called from MusicBee
        public PluginInfo Initialise(IntPtr apiPtr)
        {
            API = (MusicBeeApiInterface)Marshal.PtrToStructure(apiPtr, typeof(MusicBeeApiInterface));

            this.Initialise();

            PluginInfo info = new PluginInfo();
            info.PluginInfoVersion = PluginInfoVersion;
            info.Name = "Lyrics Reloaded!";
            info.Description = "Lyrics loading done properly! - https://github.com/quickwango/LyricsReloaded";
            info.Author = "Phillip Schichtel <Quick_Wango>";
            info.TargetApplication = "MusicBee";
            info.Type = PluginType.LyricsRetrieval;
            info.VersionMajor = 1;
            info.VersionMinor = 0;
            info.Revision = 1;
            info.MinInterfaceVersion = MinInterfaceVersion;
            info.MinApiRevision = MinApiRevision;
            info.ReceiveNotifications = ReceiveNotificationFlags.StartupOnly;
            info.ConfigurationPanelHeight = 0;

            return info;
        }

        public void Initialise()
        {
            if (!this.initialized)
            {
                this.initialized = true;
                this.pluginDirectory = Path.GetDirectoryName(Assembly.GetAssembly(this.GetType()).Location);
                this.loader = new LyricsLoader(20000);
                this.initFilters();
                this.initBuildInReaders();
            }
        }

        private void initFilters()
        {
            this.filters.Add("strip_html", new HtmlStripper());
            this.filters.Add("strip_links", new LinkRemover());
            this.filters.Add("entity_decode", new HtmlEntityDecoder());
            this.filters.Add("utf8_encode", new UTF8Encoder());
            this.filters.Add("br2nl", new Br2Nl());
            this.filters.Add("p2break", new P2Break());
            this.filters.Add("clean_spaces", new WhitespaceCleaner());
        }

        private void initBuildInReaders()
        {
            this.loadReaderFromResource(Properties.Resources.azlyrics_com);
            this.loadReaderFromResource(Properties.Resources.plyrics_com);
            this.loadReaderFromResource(Properties.Resources.urbanlyrics_com);
            this.loadReaderFromResource(Properties.Resources.rapgenius_com);
            this.loadReaderFromResource(Properties.Resources.oldielyrics_com);
        }

        private void loadReaderFromResource(byte[] resourceData)
        {
            this.loadReader(new StreamReader(new MemoryStream(resourceData), Encoding.UTF8));
        }

        public void loadReader(TextReader configReader)
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

            LyricsReader reader = new LyricsReader(name, url, expressions, filters);
            this.readers.Add(reader.getName(), reader);
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
            return this.readers.Keys.ToArray();
        }

        public String RetrieveLyrics(String source, String artist, String title, String album, bool preferSynced, String provider)
        {
            LyricsReader reader;
            try
            {
                reader = this.readers[provider];
            }
            catch (KeyNotFoundException e)
            {
                return null;
            }

            String url = reader.constructUrl(artist, title, album, preferSynced);

            Console.WriteLine("URL: " + url);

            LyricsResponse response = this.loader.loadContent(url, "USER_AGENT");

            return reader.processContent(response.getContent(), response.getEncoding());
        }

        public void ReceiveNotification(String source, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.PluginStartup:
                    String proxySetting = API.Setting_GetWebProxy();
                    if (proxySetting != null)
                    {
                        string[] raw = proxySetting.Split(Convert.ToChar(0));
                        WebProxy proxy = new WebProxy(raw[0]);
                        if (raw.Length >= 3)
                        {
                            proxy.Credentials = new NetworkCredential(raw[1], raw[2]);
                        }
                        this.loader.setProxy(proxy);
                    }
                    break;
            }
        }
    }
}
