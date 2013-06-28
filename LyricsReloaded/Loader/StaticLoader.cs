using System;
using System.Text;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace CubeIsland.LyricsReloaded.Provider.Loader
{
    public class StaticLoader : LyricsLoader
    {
        private readonly LyricsReloaded lyricsReloaded;
        private readonly string name;
        private readonly string urlTemplate;
        private readonly Pattern pattern;
        private readonly WebClient client;

        public StaticLoader(LyricsReloaded lyricsReloaded, WebClient client, string name, string urlTemplate, Pattern pattern)
        {
            this.lyricsReloaded = lyricsReloaded;
            this.name = name;
            this.urlTemplate = urlTemplate;
            this.pattern = pattern;
            this.client = client;
        }

        public string getName()
        {
            return this.name;
        }

        private string constructUrl(Dictionary<string, string> variables)
        {
            string url = this.urlTemplate;

            foreach (KeyValuePair<string, string> entry in variables)
            {
                url = url.Replace("{" + entry.Key + "}", entry.Value);
            }

            return url;
        }

        public Lyrics getLyrics(Dictionary<string, string> variables)
        {
            string url = this.constructUrl(variables);

            this.lyricsReloaded.getLogger().debug("The constructed URL: {0}", url);

            WebResponse response = this.client.get(url);
            string lyrics = this.pattern.apply(response.getContent());

            return new Lyrics(lyrics, response.getEncoding());
        }
    }

    public class StaticLoaderFactory : LyricsLoaderFactory
    {
        private static class Node
        {
            public static readonly YamlScalarNode URL = new YamlScalarNode("url");
            public static readonly YamlScalarNode PATTERN = new YamlScalarNode("pattern");

            public static class Pattern
            {
                public static readonly YamlScalarNode REGEX = new YamlScalarNode("regex");
                public static readonly YamlScalarNode OPTIONS = new YamlScalarNode("options");
            }
        }

        private readonly LyricsReloaded lyricsReloaded;
        private readonly WebClient webClient;

        public StaticLoaderFactory(LyricsReloaded lyricsReloaded)
        {
            this.lyricsReloaded = lyricsReloaded;
            this.webClient = new WebClient(lyricsReloaded, 20000);
        }

        public string getName()
        {
            return "static";
        }

        public LyricsLoader newLoader(string name, YamlMappingNode configuration)
        {
            YamlNode node;
            IDictionary<YamlNode, YamlNode> configNodes = configuration.Children;

            string url;
            node = (configNodes.ContainsKey(Node.URL) ? configNodes[Node.URL] : null);
            if (node is YamlScalarNode)
            {
                url = ((YamlScalarNode)node).Value;
            }
            else
            {
                throw new InvalidConfigurationException("No URL specified!");
            }

            string regex;
            string regexOptions = "";

            node = (configNodes.ContainsKey(Node.PATTERN) ? configNodes[Node.PATTERN] : null);
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
                YamlMappingNode patternConfig = (YamlMappingNode)node;
                node = (configNodes.ContainsKey(Node.Pattern.REGEX) ? configNodes[Node.Pattern.REGEX] : null);
                if (!(node is YamlScalarNode))
                {
                    throw new InvalidConfigurationException("Invalid regex value!");
                }
                regex = ((YamlScalarNode)node).Value;

                node = (configNodes.ContainsKey(Node.Pattern.OPTIONS) ? configNodes[Node.Pattern.OPTIONS] : null);
                if (node is YamlScalarNode)
                {
                    regexOptions = ((YamlScalarNode)node).Value;
                }
            }
            else
            {
                throw new InvalidConfigurationException("No pattern specified!");
            }

            return new StaticLoader(this.lyricsReloaded, this.webClient, name, url, new Pattern(regex, regexOptions));
        }
    }
}
