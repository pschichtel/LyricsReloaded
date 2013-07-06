using System;
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

            Console.WriteLine("Url: " + url);

            lyricsReloaded.getLogger().debug("The constructed URL: {0}", url);

            WebResponse response = client.get(url);
            Console.WriteLine("Raw content:\n" + response.getContent());
            String lyrics = pattern.apply(response.getContent());
            if (lyrics == null)
            {
                lyricsReloaded.getLogger().warn("The pattern {0} didn't match!", pattern);
                return null;
            }

            return new Lyrics(lyrics, response.getEncoding());
        }
    }

    public class StaticLoaderFactory : LyricsLoaderFactory
    {
        private static class Node
        {
            public static readonly YamlScalarNode URL = new YamlScalarNode("url");
            public static readonly YamlScalarNode PATTERN = new YamlScalarNode("pattern");
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

            if (!configNodes.ContainsKey(Node.PATTERN))
            {
                throw new InvalidConfigurationException("No pattern specified!");
            }

            return new StaticLoader(lyricsReloaded, webClient, url, Pattern.fromYamlNode(configNodes[Node.PATTERN]));
        }
    }
}
