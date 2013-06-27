using System;
using YamlDotNet.RepresentationModel;

namespace CubeIsland.LyricsReloaded.Provider.Loader
{
    public class StaticLoader : LyricsLoader
    {
        private readonly LyricsReloaded lyricsReloaded;
        private readonly string name;
        private readonly string urlTemplate;
        private readonly Pattern pattern;

        public StaticLoader(LyricsReloaded lyricsReloaded, string name, string urlTemplate, Pattern pattern)
        {
            this.lyricsReloaded = lyricsReloaded;
            this.name = name;
            this.urlTemplate = urlTemplate;
            this.pattern = pattern;
        }

        public string getName()
        {
            return this.name;
        }

        private string constructUrl(String artist, String title, String album)
        {
            return this.urlTemplate
                .Replace("{artist}", artist)
                .Replace("{title}", title)
                .Replace("{album}", album);
        }

        public String getLyrics(String artist, String title, String album)
        {
            return null;
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
        private readonly WebClient loader;

        public StaticLoaderFactory(LyricsReloaded lyricsReloaded)
        {
            this.lyricsReloaded = lyricsReloaded;
            this.loader = new WebClient(lyricsReloaded, 20000);
        }

        public string getName()
        {
            return "static";
        }

        public LyricsLoader newLoader(string name, YamlMappingNode configuration)
        {
            YamlNode node;

            string url;
            node = configuration.Children[Node.URL];
            if (node != null && node is YamlScalarNode)
            {
                url = ((YamlScalarNode)node).Value;
            }
            else
            {
                throw new InvalidConfigurationException("No URL specified!");
            }

            string regex;
            string regexOptions = "";

            node = configuration.Children[Node.PATTERN];
            if (node is YamlScalarNode)
            {
                regex = ((YamlScalarNode)node).Value;
            }
            else if (node is YamlMappingNode)
            {
                YamlMappingNode patternConfig = (YamlMappingNode)node;
                node = patternConfig.Children[Node.Pattern.REGEX];
                if (node == null || !(node is YamlScalarNode))
                {
                    throw new InvalidConfigurationException("Invalid regex value!");
                }
                regex = ((YamlScalarNode)node).Value;

                node = patternConfig.Children[Node.Pattern.OPTIONS];
                if (node != null && node is YamlScalarNode)
                {
                    regexOptions = ((YamlScalarNode)node).Value;
                }
            }
            else
            {
                throw new InvalidConfigurationException("No pattern specified!");
            }

            return new StaticLoader(this.lyricsReloaded, name, url, new Pattern(regex, regexOptions));
        }
    }
}
