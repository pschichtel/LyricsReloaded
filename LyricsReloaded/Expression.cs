using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CubeIsland.LyricsReloaded
{
    public class Expression
    {
        private Regex regex;
        private int group;

        public Expression(string regex)
        {
            this.regex = new Regex(regex, RegexOptions.Compiled);
            this.group = this.regex.GroupNumberFromName("lyrics");
        }

        public string apply(string content)
        {
            Match match = this.regex.Match(content);
            return match.Groups[1].ToString();
        }
    }
}
