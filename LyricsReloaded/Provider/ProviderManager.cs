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

using CubeIsland.LyricsReloaded.Filters;
using CubeIsland.LyricsReloaded.Provider.Loader;
using CubeIsland.LyricsReloaded.Validation;
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
            public static readonly YamlScalarNode QUALITY = new YamlScalarNode("quality");
            public static readonly YamlScalarNode RATE_LIMIT = new YamlScalarNode("rate-limit");
            public static readonly YamlScalarNode LOADER = new YamlScalarNode("loader");
            public static readonly YamlScalarNode POST_FILTERS = new YamlScalarNode("post-filters");
            public static readonly YamlScalarNode VALIDATIONS = new YamlScalarNode("validations");
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
        private readonly object providerLock = new object();
        private readonly Dictionary<string, Provider> providers;
        private readonly Dictionary<string, LyricsLoaderFactory> loaderFactories;
        private readonly Dictionary<string, Filter> filters;
        private readonly Dictionary<string, Validator> validators;

        private static readonly Dictionary<string, Variable.Type> VARIABLE_TYPES = new Dictionary<string, Variable.Type> {
            {"artist", Variable.Type.ARTIST},
            {"title", Variable.Type.TITLE},
            {"album", Variable.Type.ALBUM}
        };

        public ProviderManager(LyricsReloaded lyricsReloaded)
        {
            this.lyricsReloaded = lyricsReloaded;
            logger = lyricsReloaded.getLogger();
            providers = new Dictionary<string, Provider>();
            loaderFactories = new Dictionary<string, LyricsLoaderFactory>();
            filters = new Dictionary<string, Filter>();
            validators = new Dictionary<string, Validator>();

            loadFilters();
            loadValidators();
        }

        private void loadFilters()
        {
            Type filterType = typeof(Filter);
            foreach (Type type in Assembly.GetAssembly(filterType).GetTypes())
            {
                if (filterType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                {
                    Filter filter = (Filter)Activator.CreateInstance(type);
                    filters.Add(filter.getName(), filter);
                }
            }
        }

        private void loadValidators()
        {
            Type validatorType = typeof(Validator);
            foreach (Type type in Assembly.GetAssembly(validatorType).GetTypes())
            {
                if (validatorType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                {
                    Validator validator = (Validator)Activator.CreateInstance(type);
                    validators.Add(validator.getName(), validator);
                }
            }
        }

        public void registerLoaderFactory(LyricsLoaderFactory factory)
        {
            if (factory != null)
            {
                loaderFactories.Add(factory.getName().ToLower(), factory);
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
            if (providerName == null)
            {
                return null;
            }
            lock (providerLock)
            {
                if (providers.ContainsKey(providerName))
                {
                    return providers[providerName];
                }
            }
            return null;
        }

        public IList<Provider> getProviders()
        {
            lock (providerLock)
            {
                List<Provider> providerList = new List<Provider>(providers.Values);
                providerList.Sort();
                return providerList;
            }
        }

        public void loadProvider(FileInfo fileInfo)
        {
            using (FileStream stream = fileInfo.OpenRead())
            {
                logger.debug("Loading config from file: {0}", fileInfo.FullName);
                loadProvider(new StreamReader(stream));
            }
        }

        public void loadProvider(String resourceText)
        {
            loadProvider(new StringReader(resourceText));
        }

        public void loadProvider(byte[] resourceData)
        {
            loadProvider(new StreamReader(new MemoryStream(resourceData), Encoding.UTF8));
        }

        /// <summary>
        /// Loads a configuration from any TextReader
        /// </summary>
        /// <param name="configReader">the reader</param>
        /// <exception cref="InvalidConfigurationException">if an error occurs during configuration loading</exception>
        public void loadProvider(TextReader configReader)
        {
            YamlStream yaml = new YamlStream();
            try
            {
                yaml.Load(configReader);
            }
            catch (Exception e)
            {
                throw new InvalidConfigurationException(e.Message, e);
            }

            YamlNode node;
            IDictionary<YamlNode, YamlNode> rootNodes = ((YamlMappingNode)yaml.Documents[0].RootNode).Children;

            string loaderName;
            node = (rootNodes.ContainsKey(Node.LOADER) ? rootNodes[Node.LOADER] : null);
            if (node is YamlScalarNode)
            {
                loaderName = ((YamlScalarNode)node).Value.Trim().ToLower();
            }
            else
            {
                loaderName = "static";
            }
            if (!loaderFactories.ContainsKey(loaderName))
            {
                throw new InvalidConfigurationException("Unknown provider type " + loaderName + ", skipping");
            }
            LyricsLoaderFactory loaderFactory = loaderFactories[loaderName];

            node = (rootNodes.ContainsKey(Node.NAME) ? rootNodes[Node.NAME] : null);
            if (!(node is YamlScalarNode))
            {
                throw new InvalidConfigurationException("No provider name given!");
            }
            string name = ((YamlScalarNode)node).Value.Trim();

            ushort quality = 50;
            node = (rootNodes.ContainsKey(Node.QUALITY) ? rootNodes[Node.QUALITY] : null);
            if (node is YamlScalarNode)
            {
                try
                {
                    quality = Convert.ToUInt16(((YamlScalarNode)node).Value.Trim());
                }
                catch (FormatException e)
                {
                    throw new InvalidConfigurationException("Invalid quality value given, only positive numbers >= 0 are allowed!", e);
                }
            }

            RateLimit rateLimit = null;
            node = (rootNodes.ContainsKey(Node.RATE_LIMIT) ? rootNodes[Node.RATE_LIMIT] : null);
            if (node is YamlScalarNode)
            {
                rateLimit = RateLimit.parse(((YamlScalarNode) node).Value.Trim());
            }

            node = (rootNodes.ContainsKey(Node.VARIABLES) ? rootNodes[Node.VARIABLES] : null);
            Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
            if (node is YamlMappingNode)
            {
                foreach (KeyValuePair<YamlNode, YamlNode> preparationEntry in ((YamlMappingNode)node).Children)
                {
                    node = preparationEntry.Key;
                    if (node is YamlScalarNode)
                    {
                        string variableName = ((YamlScalarNode)node).Value.ToLower();

                        if (variables.ContainsKey(variableName))
                        {
                            throw InvalidConfigurationException.fromFormat("{0}: Variable already defined!", variableName);
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
                                throw InvalidConfigurationException.fromFormat("{0}: Unknown variable type {1}!", variableName, typeString);
                            }
                        }
                        // value with filters expected
                        else if (node is YamlMappingNode)
                        {
                            YamlMappingNode variableConfig = (YamlMappingNode)node;

                            node = variableConfig.Children[Node.Variables.TYPE];
                            if (!(node is YamlScalarNode))
                            {
                                throw InvalidConfigurationException.fromFormat("{0}: Invalid variable type!", variableName);
                            }

                            Variable.Type type;
                            string typeString = ((YamlScalarNode)node).Value.ToLower();
                            try
                            {
                                type = VARIABLE_TYPES[typeString];
                            }
                            catch
                            {
                                throw InvalidConfigurationException.fromFormat("{0}: Unknown variable type {1}!", variableName, typeString);
                            }

                            FilterCollection filterCollection;

                            node = (variableConfig.Children.ContainsKey(Node.Variables.FILTERS) ? variableConfig.Children[Node.Variables.FILTERS] : null);
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
                                    throw InvalidConfigurationException.fromFormat("{0}: Unknown variable {1} referenced!", variableName, referencedVar);
                                }
                            }
                            // a list of filters
                            else if (node is YamlSequenceNode)
                            {
                                filterCollection = FilterCollection.parseList((YamlSequenceNode)node, filters);
                            }
                            else
                            {
                                throw new InvalidConfigurationException("Invalid filter option specified!");
                            }

                            variables.Add(variableName, new Variable(variableName, type, filterCollection));
                        }
                    }
                    else
                    {
                        throw new InvalidConfigurationException("Invalid configration, aborting the configuration.");
                    }
                }
            }

            foreach (KeyValuePair<string, Variable.Type> entry in VARIABLE_TYPES)
            {
                if (!variables.ContainsKey(entry.Key))
                {
                    variables.Add(entry.Key, new Variable(entry.Key, entry.Value));
                }
            }

            node = (rootNodes.ContainsKey(Node.POST_FILTERS) ? rootNodes[Node.POST_FILTERS] : null);
            FilterCollection postFilters;
            if (node is YamlSequenceNode)
            {
                postFilters = FilterCollection.parseList((YamlSequenceNode)node, filters);
            }
            else
            {
                postFilters = new FilterCollection();
            }

            node = (rootNodes.ContainsKey(Node.VALIDATIONS) ? rootNodes[Node.VALIDATIONS] : null);
            ValidationCollection validations;
            if (node is YamlSequenceNode)
            {
                validations = ValidationCollection.parseList((YamlSequenceNode)node, validators);
            }
            else
            {
                validations = new ValidationCollection();
            }

            YamlMappingNode configNode;
            node = (rootNodes.ContainsKey(Node.CONFIG) ? rootNodes[Node.CONFIG] : null);
            if (node is YamlMappingNode)
            {
                configNode = (YamlMappingNode)node;
            }
            else
            {
                configNode = new YamlMappingNode();
            }

            LyricsLoader loader = loaderFactory.newLoader(configNode);

            Provider provider = new Provider(lyricsReloaded, name, quality, variables, postFilters, validations, loader, rateLimit);
            logger.info("Provider loaded: " + provider.getName());

            lock (providerLock)
            {
                if (providers.ContainsKey(provider.getName()))
                {
                    logger.info("The provider {0} does already exist and will be replaced.", provider.getName());
                    providers.Remove(provider.getName());
                }
                providers.Add(provider.getName(), provider);
            }
        }

        public void clean()
        {
            lock (providerLock)
            {
                providers.Clear();
                loaderFactories.Clear();
                filters.Clear();
            }
        }

        public void shutdown()
        {
            lock (providerLock)
            {
                foreach (Provider provider in providers.Values)
                {
                    provider.shutdown();
                }
                clean();
            }
        }
    }

    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(String message) : base(message)
        {}

        public InvalidConfigurationException(string message, Exception innerException) : base(message, innerException)
        {}

        public static InvalidConfigurationException fromFormat(string format, params object[] args)
        {
            return new InvalidConfigurationException(String.Format(format, args));
        }
    }
}
