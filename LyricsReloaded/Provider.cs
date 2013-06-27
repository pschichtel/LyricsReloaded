using CubeIsland.LyricsReloaded.Filters;
using CubeIsland.LyricsReloaded.Provider.Loader;
using System;
using System.Text;
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
            Dictionary<string, string> variableValues = new Dictionary<string, string>(this.variables.Count);

            Variable var;
            foreach (KeyValuePair<string, Variable> entry in this.variables)
            {
                var = entry.Value;
                switch (var.getType())
                {
                    case Variable.Type.ARTIST:
                        variableValues.Add(entry.Key, var.process(artist, Encoding.UTF8));
                        break;
                    case Variable.Type.TITLE:
                        variableValues.Add(entry.Key, var.process(title, Encoding.UTF8));
                        break;
                    case Variable.Type.ALBUM:
                        variableValues.Add(entry.Key, var.process(album, Encoding.UTF8));
                        break;
                }
            }

            Lyrics lyrics = this.loader.getLyrics(variableValues);

            if (lyrics == null)
            {
                return null;
            }

            return this.postFilters.applyFilters(lyrics.getContent(), lyrics.getEncoding());
        }
    }
}
