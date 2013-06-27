using CubeIsland.LyricsReloaded.Filters;
using System.Text;

namespace CubeIsland.LyricsReloaded
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
            return this.name;
        }

        public Type getType()
        {
            return this.type;
        }

        public FilterCollection getFilters()
        {
            return this.filters;
        }

        public string process(string input, Encoding encoding)
        {
            if (this.filters == null)
            {
                return input;
            }
            return this.filters.applyFilters(input, encoding);
        }

        public enum Type
        {
            ARTIST, TITLE, ALBUM
        }
    }
}
