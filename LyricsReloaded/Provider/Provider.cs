using CubeIsland.LyricsReloaded.Filters;
using CubeIsland.LyricsReloaded.Validation;
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
        private readonly ValidationCollection validations;
        private readonly LyricsLoader loader;

        private volatile int counter;
        private readonly int maxCount;

        public Provider(string name, Dictionary<string, Variable> variables, FilterCollection postFilters, ValidationCollection validations, LyricsLoader loader, int maxCount = -1)
        {
            this.name = name;
            this.variables = variables;
            this.postFilters = postFilters;
            this.validations = validations;
            this.loader = loader;

            this.counter = 0;
            this.maxCount = maxCount;
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

        public ValidationCollection getValidations()
        {
            return this.validations;
        }

        public LyricsLoader getLoader()
        {
            return this.loader;
        }

        public String getLyrics(String artist, String title, String album)
        {
            if (this.counter == this.maxCount)
            {
                return null;
            }

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

            this.counter++;
            Lyrics lyrics = this.loader.getLyrics(variableValues);

            if (lyrics == null)
            {
                return null;
            }

            string filteredLyrics = this.postFilters.applyFilters(lyrics.getContent(), lyrics.getEncoding());

            if (!this.validations.executeValidations(filteredLyrics))
            {
                return null;
            }

            return filteredLyrics;
        }
    }
}
