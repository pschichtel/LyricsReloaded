using CubeIsland.LyricsReloaded.Filters;
using CubeIsland.LyricsReloaded.Provider.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace CubeIsland.LyricsReloaded.Provider
{
    public class ProviderManager
    {
        private static class Node
        {
            public static readonly YamlScalarNode NAME = new YamlScalarNode("name");
            public static readonly YamlScalarNode LOADER = new YamlScalarNode("loader");
            public static readonly YamlScalarNode POST_FILTERS = new YamlScalarNode("post-filters");
            public static readonly YamlScalarNode CONFIG = new YamlScalarNode("config");

            public static readonly YamlScalarNode VARIABLES = new YamlScalarNode("variables");
            public static class Variables
            {
                public static readonly YamlScalarNode TYPE = new YamlScalarNode("type");
                public static readonly YamlScalarNode FILTERS = new YamlScalarNode("filters");
            }
        }

        private readonly LyricsReloaded lyricsReloaded;
        private readonly Logger logger;
        private readonly Dictionary<string, Provider> providers;
        private readonly Dictionary<string, LyricsLoaderFactory> loaderFactories;
        private readonly Dictionary<string, Filter> filters;

        private static readonly Dictionary<string, Variable.Type> VARIABLE_TYPES = new Dictionary<string, Variable.Type>() {
            {"artist", Variable.Type.ARTIST},
            {"title", Variable.Type.TITLE},
            {"album", Variable.Type.ALBUM}
        };

        public ProviderManager(LyricsReloaded lyricsReloaded)
        {
            this.lyricsReloaded = lyricsReloaded;
            this.logger = lyricsReloaded.getLogger();
            this.providers = new Dictionary<string, Provider>();
            this.loaderFactories = new Dictionary<string, LyricsLoaderFactory>();
            this.filters = new Dictionary<string, Filter>();

            this.loadFilters();
        }

        private void loadFilters()
        {
            Type filterType = typeof(Filter);
            foreach (Type type in Assembly.GetAssembly(filterType).GetTypes())
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

        public void registerLoaderFactory(LyricsLoaderFactory factory)
        {
            if (factory != null)
            {
                this.loaderFactories.Add(factory.getName().ToLower(), factory);
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

        public Provider getProvider(string providerName)
        {
            providerName = providerName.ToLower();
            if (this.providers.ContainsKey(providerName))
            {
                return this.providers[providerName];
            }
            return null;
        }

        public Dictionary<string, Provider> getProviders()
        {
            return new Dictionary<string, Provider>(this.providers);
        }

        public void loadProvider(FileInfo fileInfo)
        {
            using (FileStream stream = fileInfo.OpenRead())
            {
                this.loadProvider(new StreamReader(stream));
            }
        }

        public void loadProvider(byte[] resourceData)
        {
            this.loadProvider(new StreamReader(new MemoryStream(resourceData), Encoding.UTF8));
        }

        public void loadProvider(TextReader configReader)
        {
            YamlStream yaml = new YamlStream();
            yaml.Load(configReader);

            YamlNode node;
            YamlMappingNode root = (YamlMappingNode)yaml.Documents[0].RootNode;

            string loaderName;
            node = root.Children[Node.LOADER];
            if (node != null && node is YamlScalarNode)
            {
                loaderName = ((YamlScalarNode)node).Value.ToLower();
            }
            else
            {
                loaderName = "static";
            }
            if (!this.loaderFactories.ContainsKey(loaderName))
            {
                this.logger.warn("Unknown provider type {0}, skipping", loaderName);
            }
            LyricsLoaderFactory factory = this.loaderFactories[loaderName];

            node = root.Children[Node.NAME];
            if (!(node is YamlScalarNode))
            {
                throw new InvalidConfigurationException("Invalid configuration");
            }
            string name = ((YamlScalarNode)node).Value.ToLower();

            node = root.Children[Node.VARIABLES];
            Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
            foreach (KeyValuePair<string, Variable.Type> entry in VARIABLE_TYPES)
            {
                variables.Add(entry.Key, new Variable(entry.Key, entry.Value));
            }

            if (node != null && node is YamlMappingNode)
            {
                string variableName;
                foreach (KeyValuePair<YamlNode, YamlNode> preparationEntry in ((YamlMappingNode)node).Children)
                {
                    node = preparationEntry.Key;
                    if (node is YamlScalarNode)
                    {
                        variableName = ((YamlScalarNode)node).Value.ToLower();

                        if (variables.ContainsKey(variableName))
                        {
                            this.logger.error("{0}: Variable already defined!", variableName);
                            return;
                        }

                        node = preparationEntry.Value;
                        // variable value without filters
                        if (node is YamlScalarNode)
                        {
                            string typeString = ((YamlScalarNode)node).Value.ToLower();
                            try
                            {
                                Variable.Type variableType = VARIABLE_TYPES[typeString];
                                variables.Add(variableName, new Variable(variableName, variableType));
                            }
                            catch
                            {
                                this.logger.error("{0}: Unknown variable type {1}!", variableName, typeString);
                                return;
                            }
                        }
                        // value with filters expected
                        else if (node is YamlMappingNode)
                        {
                            YamlMappingNode variableConfig = (YamlMappingNode)node;

                            node = variableConfig.Children[Node.Variables.TYPE];
                            if (!(node is YamlScalarNode))
                            {
                                this.logger.error("{0}: Invalid variable type!", variableName);
                                return;
                            }

                            Variable.Type type;
                            string typeString = ((YamlScalarNode)node).Value.ToLower();
                            try
                            {
                                type = VARIABLE_TYPES[typeString];
                            }
                            catch
                            {
                                this.logger.error("{0}: Unknown variable type {1}!", variableName, typeString);
                                return;
                            }

                            FilterCollection filterCollection = null;

                            node = variableConfig.Children[Node.Variables.FILTERS];
                            // variable reference
                            if (node is YamlScalarNode)
                            {
                                string referencedVar = ((YamlScalarNode)node).Value.ToLower();
                                try
                                {
                                    filterCollection = variables[referencedVar].getFilters();
                                }
                                catch
                                {
                                    this.logger.error("{0}: Unknown variable {1} referenced!", variableName, referencedVar);
                                    return;
                                }
                            }
                            // a list of filters
                            else if (node is YamlSequenceNode)
                            {
                                filterCollection = FilterCollection.parseList((YamlSequenceNode)node, this.filters);
                            }
                            else
                            {
                                this.logger.warn("Invalid filter option specified!");
                                return;
                            }

                            variables.Add(variableName, new Variable(variableName, type, filterCollection));
                        }
                    }
                    else
                    {
                        this.logger.warn("Invalid configration, aborting the configuration.");
                        return;
                    }
                }
            }

            node = root.Children[Node.POST_FILTERS];
            FilterCollection postFilters = new FilterCollection();
            if (node is YamlSequenceNode)
            {
                FilterCollection.parseList((YamlSequenceNode)node, this.filters);
            }

            YamlMappingNode configNode;
            node = root.Children[Node.CONFIG];
            if (node != null && node is YamlMappingNode)
            {
                configNode = (YamlMappingNode)node;
            }
            else
            {
                configNode = new YamlMappingNode();
            }

            LyricsLoader loader = factory.newLoader(name, configNode);

            Provider provider = new Provider(name, variables, postFilters, loader);
            this.logger.info("Provider loaded: " + provider.getName());

            lock (this.providers)
            {
                if (this.providers.ContainsKey(provider.getName()))
                {
                    this.logger.info("The provider {0} does already exist and will be replaced.", provider.getName());
                    this.loaderFactories.Remove(provider.getName());
                }
                this.providers.Add(provider.getName(), provider);
            }
        }

        public void clean()
        {
            this.providers.Clear();
            this.loaderFactories.Clear();
            this.filters.Clear();
        }
    }

    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(String message) : base(message)
        {}
    }
}
