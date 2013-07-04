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
using System.Text;

namespace CubeIsland.LyricsReloaded.Provider
{
    public class Variable
    {
        private readonly string name;
        private readonly Type type;
        private readonly FilterCollection filters;

        public Variable(string name, Type type, FilterCollection filters = null)
        {
            this.name = name;
            this.type = type;
            if (filters == null || filters.getSize() <= 0)
            {
                this.filters = null;
            }
            else
            {
                this.filters = filters;
            }
        }

        public string getName()
        {
            return name;
        }

        public Type getType()
        {
            return type;
        }

        public FilterCollection getFilters()
        {
            return filters;
        }

        public string process(string input, Encoding encoding)
        {
            if (filters == null)
            {
                return input;
            }
            return filters.applyFilters(input, encoding);
        }

        public enum Type
        {
            ARTIST, TITLE, ALBUM
        }
    }
}
