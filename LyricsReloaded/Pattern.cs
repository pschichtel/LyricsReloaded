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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CubeIsland.LyricsReloaded
{
    public class Pattern
    {
        private readonly Regex regex;

        public Pattern(string regex, string options)
        {
            this.regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | optionStringToRegexOptions(options));
        }

        public string apply(string content)
        {
            Match match = this.regex.Match(content);
            if (match.Success)
            {
                return match.Groups["lyrics"].ToString();
            }
            else
            {
                throw new Exception("The pattern didn't match: " + this.regex.ToString());
            }
        }

        private static readonly Dictionary<char, RegexOptions> regexOptionMap = new Dictionary<char, RegexOptions> {
            {'i', RegexOptions.IgnoreCase},
            {'s', RegexOptions.Singleline},
            {'m', RegexOptions.Multiline}
        };

        public static RegexOptions optionStringToRegexOptions(string optionString)
        {
            RegexOptions options = RegexOptions.None;

            foreach (char c in optionString)
            {
                if (regexOptionMap.ContainsKey(c))
                {
                    options |= regexOptionMap[c];
                }
            }

            return options;
        }
    }
}
