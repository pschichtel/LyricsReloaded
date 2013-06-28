using CubeIsland.LyricsReloaded.Provider;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace CubeIsland.LyricsReloaded.Filters
{
    public class FilterCollection
    {
        private static class Node
        {
            public static readonly YamlScalarNode NAME = new YamlScalarNode("name");
            public static readonly YamlScalarNode ARGS = new YamlScalarNode("args");
        }

        private readonly LinkedList<KeyValuePair<Filter, string[]>> filters;

        public FilterCollection()
        {
            this.filters = new LinkedList<KeyValuePair<Filter,string[]>>();
        }

        public void Add(KeyValuePair<Filter, string[]> filter)
        {
            this.filters.AddLast(filter);
        }

        public void Add(Filter filter, string[] args)
        {
            this.Add(new KeyValuePair<Filter, string[]>(filter, args));
        }

        public int getSize()
        {
            return this.filters.Count;
        }

        public string applyFilters(string content, Encoding encoding)
        {
            foreach (KeyValuePair<Filter, string[]> entry in this.filters)
            {
                content = entry.Key.filter(content, entry.Value, encoding);
            }
            return content;
        }

        public static FilterCollection parseList(YamlSequenceNode list, Dictionary<string, Filter> filterMap)
        {
            FilterCollection collection = new FilterCollection();

            if (list != null)
            {
                foreach (YamlNode node in list.Children)
                {
                    parseFilterNode(collection, filterMap, node);
                }
            }

            return collection;
        }
        private static void parseFilterNode(FilterCollection filterCollection, Dictionary<string, Filter> filterMap, YamlNode node)
        {
            string name;
            string[] args;
            if (node is YamlScalarNode)
            {
                name = ((YamlScalarNode)node).Value.ToLower();
                args = new string[0];
            }
            else if (node is YamlSequenceNode)
            {
                IEnumerator<YamlNode> it = ((YamlSequenceNode)node).Children.GetEnumerator();
                if (!it.MoveNext())
                {
                    throw new InvalidConfigurationException("An empty list as a filter is not valid!");
                }
                node = it.Current;
                if (!(node is YamlScalarNode))
                {
                    throw new InvalidConfigurationException("Filter definitions as a list my only contain strings!");
                }
                name = ((YamlScalarNode)node).Value.ToLower();
                args = readFilterArgs(it);
            }
            else if (node is YamlMappingNode)
            {
                YamlMappingNode filterConfig = (YamlMappingNode)node;
                IDictionary<YamlNode, YamlNode> childNodes = filterConfig.Children;
                node = (childNodes.ContainsKey(Node.NAME) ? childNodes[Node.NAME] : null);
                if (!(node is YamlScalarNode))
                {
                    throw new InvalidConfigurationException("The filter name is missing or invalid!");
                }
                name = ((YamlScalarNode)node).Value.ToLower();

                node = (childNodes.ContainsKey(Node.ARGS) ? childNodes[Node.ARGS] : null);
                if (node is YamlSequenceNode)
                {
                    args = readFilterArgs(((YamlSequenceNode)node).Children.GetEnumerator());
                }
                else
                {
                    args = new string[0];
                }
            }
            else
            {
                throw new InvalidConfigurationException("Invalid filter configuration");
            }

            if (!filterMap.ContainsKey(name))
            {
                throw new InvalidConfigurationException("Unknown filter " + name);
            }


            filterCollection.Add(filterMap[name], args);
        }

        private static string[] readFilterArgs(IEnumerator<YamlNode> it)
        {
            LinkedList<string> args = new LinkedList<string>();

            YamlNode node;
            while (it.MoveNext())
            {
                node = it.Current;
                if (!(node is YamlScalarNode))
                {
                    throw new InvalidConfigurationException("Filter args may only be strings!");
                }
                args.AddLast(((YamlScalarNode)node).Value);
            }

            string[] argArray = new string[args.Count];
            if (argArray.Length == 0)
            {
                return argArray;
            }

            args.CopyTo(argArray, 0);
            return argArray;
        }
    }
}
