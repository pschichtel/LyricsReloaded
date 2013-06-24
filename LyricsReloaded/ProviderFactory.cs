using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CubeIsland.LyricsReloaded.Filters;
using YamlDotNet.RepresentationModel;

namespace CubeIsland.LyricsReloaded.Loaders
{
    public interface LoaderFactory
    {
        LyricsLoader newLoader(string name, YamlMappingNode configuration);
    }

    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(String message) : base(message)
        {}
    }
}
