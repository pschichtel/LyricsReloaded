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
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using CubeIsland.LyricsReloaded.Provider;

namespace CubeIsland.LyricsReloaded
{
    public class Pattern
    {
        private static class Node
        {
            public static readonly YamlScalarNode REGEX = new YamlScalarNode("regex");
            public static readonly YamlScalarNode OPTIONS = new YamlScalarNode("options");
        }

        private const RegexOptions DEFAULT_OPTIONS = RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;

        private readonly Regex regex;

        public Pattern(Regex regex)
        {
            this.regex = regex;
        }

        public Pattern(string regex, string options) : this(new Regex(regex, DEFAULT_OPTIONS | regexOptionsFromString(options)))
        {}

        public String apply(string content)
        {
            Match match = regex.Match(content);
            if (match.Success)
            {
                return match.Groups["lyrics"].ToString();
            }
            return null;
        }

        public static Pattern fromYamlNode(YamlNode node)
        {
            if (node == null)
            {
                return null;
            }
            return new Pattern(regexFromYamlNode(node, DEFAULT_OPTIONS));
        }

        public static Regex regexFromYamlNode(YamlNode node, RegexOptions options)
        {
            string regex;
            string regexOptions = "";
            if (node is YamlScalarNode)
            {
                regex = ((YamlScalarNode)node).Value;
            }
            else if (node is YamlSequenceNode)
            {
                IEnumerator<YamlNode> it = ((YamlSequenceNode)node).Children.GetEnumerator();
                if (!it.MoveNext())
                {
                    throw new InvalidConfigurationException("The pattern needs at least the regex defined!");
                }
                if (!(it.Current is YamlScalarNode))
                {
                    throw new InvalidConfigurationException("The pattern may only contain string value!");
                }
                regex = ((YamlScalarNode)it.Current).Value;

                if (it.MoveNext() && it.Current is YamlScalarNode)
                {
                    regexOptions = ((YamlScalarNode)it.Current).Value;
                }
            }
            else if (node is YamlMappingNode)
            {
                IDictionary<YamlNode, YamlNode> patternConfig = ((YamlMappingNode)node).Children;
                node = (patternConfig.ContainsKey(Node.REGEX) ? patternConfig[Node.REGEX] : null);
                if (!(node is YamlScalarNode))
                {
                    throw new InvalidConfigurationException("Invalid regex value!");
                }
                regex = ((YamlScalarNode)node).Value;

                node = (patternConfig.ContainsKey(Node.OPTIONS) ? patternConfig[Node.OPTIONS] : null);
                if (node is YamlScalarNode)
                {
                    regexOptions = ((YamlScalarNode)node).Value;
                }
            }
            else
            {
                throw new InvalidConfigurationException("No pattern specified!");
            }

            return new Regex(regex, options | regexOptionsFromString(regexOptions));
        }

        private static readonly Dictionary<char, RegexOptions> REGEX_OPTION_MAP = new Dictionary<char, RegexOptions> {
            {'i', RegexOptions.IgnoreCase},
            {'s', RegexOptions.Singleline},
            {'m', RegexOptions.Multiline},
            {'c', RegexOptions.Compiled},
            {'x', RegexOptions.IgnorePatternWhitespace},
            {'d', RegexOptions.RightToLeft},
            {'e', RegexOptions.ExplicitCapture},
            {'j', RegexOptions.ECMAScript},
            {'l', RegexOptions.CultureInvariant}
        };

        public static RegexOptions regexOptionsFromString(string optionString)
        {
            RegexOptions options = RegexOptions.None;

            char lc;
            foreach (char c in optionString)
            {
                lc = Char.ToLower(c);
                if (REGEX_OPTION_MAP.ContainsKey(lc))
                {
                    RegexOptions option = REGEX_OPTION_MAP[lc];
                    if (Char.IsLower(c))
                    {
                        options |= option;
                    }
                    else
                    {
                        options &= ~option;
                    }
                }
            }

            return options;
        }

        public override string ToString()
        {
            return regex.ToString();
        }
    }
}
