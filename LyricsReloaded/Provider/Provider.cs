/*
    Copyright 2013 Phillip Schichtel

    This file is part of LyricsReloaded.

    LyricsReloaded is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    LyricsReloaded is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with LyricsReloaded. If not, see <http://www.gnu.org/licenses/>.

*/

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

            counter = 0;
            this.maxCount = maxCount;
        }

        public string getName()
        {
            return name;
        }

        public object getVariables()
        {
            return variables;
        }

        public FilterCollection getPostFilters()
        {
            return postFilters;
        }

        public ValidationCollection getValidations()
        {
            return validations;
        }

        public LyricsLoader getLoader()
        {
            return loader;
        }

        public String getLyrics(String artist, String title, String album, bool preferSynced = false)
        {
            if (counter == maxCount)
            {
                return null;
            }

            Dictionary<string, string> variableValues = new Dictionary<string, string>(variables.Count);

            Variable var;
            foreach (KeyValuePair<string, Variable> entry in variables)
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

            counter++;
            Lyrics lyrics = loader.getLyrics(variableValues);

            if (lyrics == null)
            {
                return null;
            }

            string filteredLyrics = postFilters.applyFilters(lyrics.getContent(), lyrics.getEncoding());

            if (!validations.executeValidations(filteredLyrics))
            {
                return null;
            }

            return filteredLyrics;
        }
    }
}
