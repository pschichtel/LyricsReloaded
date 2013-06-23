using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicBeePlugin;
using YamlDotNet.RepresentationModel;
using YamlDotNet.RepresentationModel.Serialization;

namespace CubeIsland.LyricsReloaded.Providers
{
    public class StaticProvider : LyricsProvider
    {
        private readonly LyricsReloaded lyricsReloaded;
        private readonly string name;
        private readonly string urlTemplate;
        private readonly Pattern pattern;

        public StaticProvider(LyricsReloaded lyricsReloaded, string name, string urlTemplate, Pattern pattern)
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

    public class StaticProviderFactory : ProviderFactory
    {
        private static readonly YamlScalarNode NODE_URL = new YamlScalarNode("url");
        private static readonly YamlScalarNode NODE_PATTERN = new YamlScalarNode("pattern");
        private static readonly YamlScalarNode NODE_PATTERN_OPTIONS = new YamlScalarNode("pattern-options");

        private readonly LyricsReloaded lyricsReloaded;
        private readonly LyricsLoader loader;

        public StaticProviderFactory(LyricsReloaded lyricsReloaded)
        {
            this.lyricsReloaded = lyricsReloaded;
            this.loader = new LyricsLoader(lyricsReloaded, 20000);
        }

        public LyricsProvider newProvider(string name, YamlMappingNode configuration)
        {
            YamlNode node;

            string url;
            node = configuration.Children[NODE_URL];
            if (node != null && node is YamlScalarNode)
            {
                url = ((YamlScalarNode)node).Value;
            }
            else
            {
                throw new InvalidConfigurationException("No URL specified!");
            }

            string regex;
            node = configuration.Children[NODE_PATTERN];
            if (node != null && node is YamlScalarNode)
            {
                regex = ((YamlScalarNode)node).Value;
            }
            else
            {
                throw new InvalidConfigurationException("No pattern specified!");
            }

            string regexOptions = "";
            node = configuration.Children[NODE_PATTERN_OPTIONS];
            if (node != null && node is YamlScalarNode)
            {
                regexOptions = ((YamlScalarNode)node).Value;
            }

            Pattern pattern = new Pattern(regex, regexOptions);

            return new StaticProvider(this.lyricsReloaded, name, url, pattern);
        }
    }
}
