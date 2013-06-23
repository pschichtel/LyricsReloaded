using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CubeIsland.LyricsReloaded.Providers;
using CubeIsland.LyricsReloaded.Filters;
using MusicBeePlugin;
using YamlDotNet.RepresentationModel;
using System.Net;

namespace CubeIsland.LyricsReloaded
{
    public class LyricsReloaded
    {
        private static readonly YamlScalarNode NODE_NAME = new YamlScalarNode("name");
        private static readonly YamlScalarNode NODE_TYPE = new YamlScalarNode("type");
        private static readonly YamlScalarNode NODE_PREPARATION = new YamlScalarNode("preparation");
        private static readonly YamlScalarNode NODE_POST_FILTERS = new YamlScalarNode("post-filters");
        private static readonly YamlScalarNode NODE_CONFIG = new YamlScalarNode("config");

        private readonly string name;
        private readonly string pluginDirectory;
        private readonly Logger logger;
        private readonly Dictionary<string, ProviderFactory> providerFactories;
        private readonly Dictionary<string, LyricsProvider> providers;
        private readonly Dictionary<string, Filter> filters;
        private string userAgent;
        private WebProxy proxy;

        public LyricsReloaded(Plugin.MusicBeeApiInterface musicBee)
        {
            Assembly asm = Assembly.GetAssembly(this.GetType());
            this.name = asm.GetName().Name;
            this.pluginDirectory = Path.Combine(musicBee.Setting_GetPersistentStoragePath(), this.name);
            Directory.CreateDirectory(this.pluginDirectory);
            this.logger = new Logger(Path.Combine(this.pluginDirectory, this.name + ".log"));

            this.providerFactories = new Dictionary<string, ProviderFactory>()
            {
                {"static", new StaticProviderFactory(this)}
            };

            this.providers = new Dictionary<string, LyricsProvider>();

            this.filters = new Dictionary<string, Filter>();
            this.loadFilters();

            this.userAgent = "Firefox XY";
            this.proxy = null;
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

        public LyricsProvider getProvider(string providerName)
        {
            providerName = providerName.ToLower();
            if (this.providers.ContainsKey(providerName))
            {
                return this.providers[providerName];
            }
            return null;
        }

        public Dictionary<string, LyricsProvider> getProviders()
        {
            return new Dictionary<string, LyricsProvider>(this.providers);
        }

        public void setUserAgent(string userAgent)
        {
            this.userAgent = userAgent;
        }

        public string getUserAgent()
        {
            return this.userAgent;
        }

        public void setProxy(WebProxy proxy)
        {
            this.proxy = proxy;
        }

        public WebProxy getProxy()
        {
            return this.proxy;
        }

        #region "internal helpers"
        private void loadFilters()
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
                    { }
                }
            }
        }

        public void loadConfigurations()
        {
            this.loadDefaultConfiguration();

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

        private void loadDefaultConfiguration()
        {
            //this.loadProvider(Properties.Resources.songlyrics_com);
            //this.loadProvider(Properties.Resources.metrolyrics_com);
            //this.loadProvider(Properties.Resources.letras_mus_br);
            //this.loadProvider(Properties.Resources.teksty_org);
            //this.loadProvider(Properties.Resources.tekstowo_pl);
            //this.loadProvider(Properties.Resources.azlyrics_com);
            //this.loadProvider(Properties.Resources.plyrics_com);
            //this.loadProvider(Properties.Resources.urbanlyrics_com);
            //this.loadProvider(Properties.Resources.rapgenius_com);
            //this.loadProvider(Properties.Resources.oldielyrics_com);
        }


        private void loadProvider(byte[] resourceData)
        {
            this.loadProvider(new StreamReader(new MemoryStream(resourceData), Encoding.UTF8));
        }

        public void loadProvider(TextReader configReader)
        {
            YamlStream yaml = new YamlStream();
            yaml.Load(configReader);

            YamlNode node;
            YamlMappingNode root = (YamlMappingNode)yaml.Documents[0].RootNode;

            node = root.Children[NODE_TYPE];
            if (!(node is YamlScalarNode))
            {
                throw new InvalidConfigurationException("Invalid configuration");
            }
            string type = ((YamlScalarNode)node).Value.ToLower();
            if (!this.providerFactories.ContainsKey(type))
            {
                this.logger.warn("Unknown provider type {0}, skipping", type);
            }
            ProviderFactory factory = this.providerFactories[type];

            node = root.Children[NODE_NAME];
            if (!(node is YamlScalarNode))
            {
                throw new InvalidConfigurationException("Invalid configuration");
            }
            string name = ((YamlScalarNode)node).Value;

            node = root.Children[NODE_PREPARATION];
            Dictionary<string, FilterCollection> preparation = new Dictionary<string, FilterCollection>();
            if (node != null && node is YamlMappingNode)
            {
                string filterCollectionName;
                foreach (KeyValuePair<YamlNode, YamlNode> preparationEntry in ((YamlMappingNode)node).Children)
                {
                    node = preparationEntry.Key;
                    if (node is YamlScalarNode)
                    {
                        filterCollectionName = ((YamlScalarNode)node).Value.ToLower();

                        node = preparationEntry.Value;
                        if (node is YamlScalarNode)
                        {
                            string filterReference = ((YamlScalarNode)node).Value.ToLower();
                            if (preparation.ContainsKey(filterReference))
                            {
                                preparation.Add(filterCollectionName, preparation[filterReference]);
                            }
                            else if (filterReference.Equals("skip", StringComparison.OrdinalIgnoreCase))
                            {
                                this.logger.debug("Instead of using the skip reference, just omit it.");
                            }
                            else
                            {
                                this.logger.warn("Invalid preparation reference, skipping the section.");
                            }
                        }
                        else if (node is YamlSequenceNode)
                        {
                            FilterCollection filterCollection = new FilterCollection();
                            foreach (YamlNode listEntry in ((YamlSequenceNode)node).Children)
                            {
                                if (listEntry is YamlScalarNode)
                                {
                                    KeyValuePair<string, string[]> parsedEntry = Filter.parse(((YamlScalarNode)listEntry).Value);

                                    if (this.filters.ContainsKey(parsedEntry.Key))
                                    {
                                        filterCollection.Add(this.filters[parsedEntry.Key], parsedEntry.Value);
                                    }
                                }
                                else
                                {
                                    this.logger.warn("The filter lists may only contain strings");
                                }
                            }
                            if (filterCollection.getSize() > 0)
                            {
                                preparation.Add(filterCollectionName, filterCollection);
                            }
                        }
                    }
                    else
                    {
                        this.logger.warn("Invalid configration, skipping a preparation section.");
                    }
                }
            }

            node = root.Children[NODE_POST_FILTERS];
            FilterCollection filters = new FilterCollection();
            if (node is YamlSequenceNode)
            {
                FilterCollection filterCollection = new FilterCollection();
                foreach (YamlNode listEntry in ((YamlSequenceNode)node).Children)
                {
                    if (listEntry is YamlScalarNode)
                    {
                        KeyValuePair<string, string[]> parsedEntry = Filter.parse(((YamlScalarNode)listEntry).Value);

                        if (this.filters.ContainsKey(parsedEntry.Key))
                        {
                            filterCollection.Add(this.filters[parsedEntry.Key], parsedEntry.Value);
                        }
                    }
                    else
                    {
                        this.logger.warn("The filter lists may only contain strings");
                    }
                }
            }

            node = root.Children[NODE_CONFIG];
            if (!(node is YamlMappingNode))
            {
                this.logger.warn("The config node must be a map");
                return;
            }

            LyricsProvider provider = factory.newProvider(name, (YamlMappingNode)node);
            this.logger.info("Provider loaded: " + provider.getName());

            lock (this.providers)
            {
                if (this.providers.ContainsKey(provider.getName()))
                {
                    this.logger.info("The provider {0} does already exist and will be replaced.", provider.getName());
                    this.providerFactories.Remove(provider.getName());
                }
                this.providers.Add(provider.getName(), provider);
            }
        }
        #endregion
    }
}
