using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CubeIsland.LyricsReloaded.Filters;
using YamlDotNet.RepresentationModel;

namespace CubeIsland.LyricsReloaded.Providers
{
    public interface ProviderFactory
    {
        LyricsProvider newProvider(string name, YamlMappingNode configuration);
    }

    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(String message) : base(message)
        {}
    }
}
