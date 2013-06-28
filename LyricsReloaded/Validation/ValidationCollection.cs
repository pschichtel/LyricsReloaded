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
using System.Collections.Generic;
using CubeIsland.LyricsReloaded.Provider;
using YamlDotNet.RepresentationModel;

namespace CubeIsland.LyricsReloaded.Validation
{
    public class ValidationCollection
    {
        private static class Node
        {
            public static readonly YamlScalarNode NAME = new YamlScalarNode("name");
            public static readonly YamlScalarNode ARGS = new YamlScalarNode("args");
        }

        private readonly LinkedList<Validation> validations;

        public ValidationCollection()
        {
            this.validations = new LinkedList<Validation>();
        }

        public void Add(Validation validation)
        {
            this.validations.AddLast(validation);
        }

        public void Add(Validator validator, bool inverted, string[] args)
        {
            this.Add(new Validation(validator, inverted, args));
        }

        public int getSize()
        {
            return this.validations.Count;
        }

        public bool executeValidations(string content)
        {
            foreach (Validation validation in this.validations)
            {
                if (!validation.execute(content))
                {
                    return false;
                }
            }
            return true;
        }

        public static ValidationCollection parseList(YamlSequenceNode list, Dictionary<string, Validator> validatorMap)
        {
            ValidationCollection collection = new ValidationCollection();

            if (list != null)
            {
                foreach (YamlNode node in list.Children)
                {
                    parseFilterNode(collection, validatorMap, node);
                }
            }

            return collection;
        }
        private static void parseFilterNode(ValidationCollection filterCollection, Dictionary<string, Validator> validatorMap, YamlNode node)
        {
            string name;
            string[] args;
            if (node is YamlScalarNode)
            {
                name = ((YamlScalarNode)node).Value.Trim().ToLower();
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
                name = ((YamlScalarNode)node).Value.Trim().ToLower();
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
                name = ((YamlScalarNode)node).Value.Trim().ToLower();

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
                throw new InvalidConfigurationException("Invalid validator configuration");
            }

            bool inverted = false;
            int spaceIndex = name.IndexOf(" ", System.StringComparison.Ordinal);
            if (spaceIndex == 3 && name.Substring(0, 3).Equals("not", StringComparison.OrdinalIgnoreCase))
            {
                inverted = true;
                name = name.Substring(4).Trim();
            }

            if (!validatorMap.ContainsKey(name))
            {
                throw new InvalidConfigurationException("Unknown validator " + name);
            }


            filterCollection.Add(validatorMap[name], inverted, args);
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
