using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using YamlDotNet.RepresentationModel;

namespace CubeIsland.LyricsReloaded
{
    public class LyricsReader
    {
        private string name;
        private string url;
        private List<Expression> expressions;
        private List<Filter> filters;

        LyricsReader(string name, string url, List<Expression> expressions, List<Filter> filters)
        {
            this.name = name;
            this.url = url;
            this.expressions = expressions;
            this.filters = filters;
        }

        public string getName()
        {
            return this.name;
        }

        public string constructUrl(String artist, String title, String album, bool preferSynced)
        {
            return this.url;
        }

        public string processContent(string content)
        {
            foreach (Expression expr in this.expressions)
            {
                content = expr.apply(content);
            }
            foreach (Filter filter in this.filters)
            {
                content = filter.filter(content);
            }
            return content;
        }
    }
}
