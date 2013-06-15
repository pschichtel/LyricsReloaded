using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;
using CubeIsland.LyricsReloaded;
using System.Reflection;
using YamlDotNet.RepresentationModel;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private static const YamlScalarNode NODE_NAME = new YamlScalarNode("name");
        private static const YamlScalarNode NODE_URL = new YamlScalarNode("url");
        private static const YamlScalarNode NODE_EXPRESSIONS = new YamlScalarNode("expressions");
        private static const YamlScalarNode NODE_FILTERS = new YamlScalarNode("filters");

        public static MusicBeeApiInterface API;
        private Dictionary<String, LyricsReader> readers = new Dictionary<string, LyricsReader>();
        private Dictionary<string, Filter> filters = new Dictionary<string, Filter>();

        public PluginInfo Initialise(IntPtr apiPtr)
        {
            API = (MusicBeeApiInterface)Marshal.PtrToStructure(apiPtr, typeof(MusicBeeApiInterface));

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

        private void initFilters()
        {
            
        }

        private void initBuildInReaders()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (String resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.EndsWith(".yml"))
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        this.loadReader(new StreamReader(stream));
                    }
                }
            }
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
                tmp = this.getFilter(((YamlScalarNode)entry).Value);
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

        public void registerReader(LyricsReader reader)
        {
            this.readers.Add(reader.getName(), reader);
        }

        public String[] GetProviders()
        {
            return this.readers.Keys.ToArray();
        }

        public String RetrieveLyrics(String source, String artist, String title, String album, bool preferSynced, String provider)
        {
            LyricsReader reader = this.readers[provider];

            String url = reader.constructUrl(artist, title, album, preferSynced);

            String content = "";

            content = reader.processContent(content);

            return content;
        }

        public void ReceiveNotification(String source, NotificationType type)
        {
            switch (type)
            {
                case NotificationType.PluginStartup:
                    String proxySetting = API.Setting_GetWebProxy();
                    if (proxySetting != null)
                    {
                        // setup proxy
                    }
                    break;
            }
        }
    }
}
