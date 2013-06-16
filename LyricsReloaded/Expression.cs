using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CubeIsland.LyricsReloaded
{
    public class Expression
    {
        private readonly Regex regex;

        public Expression(string regex)
        {
            this.regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.Singleline);
        }

        public string apply(string content)
        {
            Match match = this.regex.Match(content);
            if (match.Success)
            {
                content = match.Groups["lyrics"].ToString();
            }

            return content;
        }
    }
}
