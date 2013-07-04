using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace CubeIsland.LyricsReloaded.Provider.Loader
{
    public class StaticLoader : LyricsLoader
    {
        private readonly LyricsReloaded lyricsReloaded;
        private readonly string urlTemplate;
        private readonly Pattern pattern;
        private readonly WebClient client;

        public StaticLoader(LyricsReloaded lyricsReloaded, WebClient client, string urlTemplate, Pattern pattern)
        {
            this.lyricsReloaded = lyricsReloaded;
            this.urlTemplate = urlTemplate;
            this.pattern = pattern;
            this.client = client;
        }

        private string constructUrl(Dictionary<string, string> variables)
        {
            string url = urlTemplate;

            foreach (KeyValuePair<string, string> entry in variables)
            {
                url = url.Replace("{" + entry.Key + "}", entry.Value);
            }

            return url;
        }

        public Lyrics getLyrics(Dictionary<string, string> variables)
        {
            string url = constructUrl(variables);

            lyricsReloaded.getLogger().debug("The constructed URL: {0}", url);

            WebResponse response = client.get(url);
            string lyrics = pattern.apply(response.getContent());

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
            webClient = new WebClient(lyricsReloaded, 20000);
        }

        public string getName()
        {
            return "static";
        }

        public LyricsLoader newLoader(YamlMappingNode configuration)
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
                IDictionary<YamlNode, YamlNode> patternConfig = ((YamlMappingNode)node).Children;
                node = (patternConfig.ContainsKey(Node.Pattern.REGEX) ? patternConfig[Node.Pattern.REGEX] : null);
                if (!(node is YamlScalarNode))
                {
                    throw new InvalidConfigurationException("Invalid regex value!");
                }
                regex = ((YamlScalarNode)node).Value;

                node = (patternConfig.ContainsKey(Node.Pattern.OPTIONS) ? patternConfig[Node.Pattern.OPTIONS] : null);
                if (node is YamlScalarNode)
                {
                    regexOptions = ((YamlScalarNode)node).Value;
                }
            }
            else
            {
                throw new InvalidConfigurationException("No pattern specified!");
            }

            return new StaticLoader(lyricsReloaded, webClient, url, new Pattern(regex, regexOptions));
        }
    }
}
