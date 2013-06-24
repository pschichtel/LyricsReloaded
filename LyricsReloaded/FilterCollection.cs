using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeIsland.LyricsReloaded.Filters
{
    public class FilterCollection : IEnumerable<KeyValuePair<Filter, string[]>>
    {
        private readonly LinkedList<KeyValuePair<Filter, string[]>> filters;

        public FilterCollection()
        {
            this.filters = new LinkedList<KeyValuePair<Filter,string[]>>();
        }

        public void Add(Filter filter, string[] args)
        {
            this.filters.AddLast(new KeyValuePair<Filter, string[]>(filter, args));
        }

        public int getSize()
        {
            return this.filters.Count;
        }

        IEnumerator<KeyValuePair<Filter, string[]>> IEnumerable<KeyValuePair<Filter, string[]>>.GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.filters.GetEnumerator();
        }
    }
}
