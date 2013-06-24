using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CubeIsland.LyricsReloaded.Filters;

namespace CubeIsland.LyricsReloaded
{
    public class Provider
    {
        private readonly string name;
        private readonly object variables = new object();
        private readonly FilterCollection postFilters;
        private readonly LyricsLoader loader;

        public Provider(string name, object variables, FilterCollection postFilters, LyricsLoader loader)
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
