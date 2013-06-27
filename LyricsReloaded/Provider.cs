using CubeIsland.LyricsReloaded.Filters;
using CubeIsland.LyricsReloaded.Provider.Loader;
using System;
using System.Collections.Generic;

namespace CubeIsland.LyricsReloaded.Provider
{
    public class Provider
    {
        private readonly string name;
        private readonly Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
        private readonly FilterCollection postFilters;
        private readonly LyricsLoader loader;

        public Provider(string name, Dictionary<string, Variable> variables, FilterCollection postFilters, LyricsLoader loader)
        {
            this.name = name;
            this.variables = variables;
            this.postFilters = postFilters;
            this.loader = loader;
        }

        public string getName()
        {
            return this.name;
        }

        public object getVariables()
        {
            return this.variables;
        }

        public FilterCollection getPostFilters()
        {
            return this.postFilters;
        }

        public LyricsLoader getLoader()
        {
            return this.loader;
        }

        public String getLyrics(String artist, String title, String album)
        {
            return null;
        }
    }
}
