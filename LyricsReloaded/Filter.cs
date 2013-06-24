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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Globalization;

namespace CubeIsland.LyricsReloaded.Filters
{
    public interface Filter
    {
        string getName();
        string filter(string content, string[] args, Encoding encoding);
    }

    public class FilterUtil
    {
        private FilterUtil()
        {}

        public static KeyValuePair<string, string[]> parse(string str)
        {
            string[] parts = str.Split(new char[] { ':' }, 2);

            string name = HttpUtility.UrlDecode(parts[0].ToLower(), Encoding.ASCII);

            string[] args = parts[1].Split(',');
            for (int i = 0; i < args.Length; ++i)
            {
                args[i] = HttpUtility.UrlDecode(args[i], Encoding.ASCII);
            }

            return new KeyValuePair<string, string[]>(name, args);
        }
    }

    public class HtmlStripper : Filter
    {
        private static readonly Regex STRIP_TAG_REGEX = new Regex("<[a-z]+(\\s+[a-z-]+(=(\"[^\"]*\"|'[^']*'|[^\\s\"'/>]+))?)*\\s*/?>|</[a-z]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        public string getName()
        {
            return "strip_html";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            return STRIP_TAG_REGEX.Replace(content, "");
        }
    }

    public class HtmlEntityDecoder : Filter
    {
        public string getName()
        {
            return "entity_decode";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            return HttpUtility.HtmlDecode(content);
        }
    }

    public class LinkRemover : Filter
    {
        private static readonly Regex STRIP_LINKS_REGEX = new Regex("https?://.*?(\\s|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string getName()
        {
            return "strip_links";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            return STRIP_LINKS_REGEX.Replace(content, "$1");
        }
    }

    public class UTF8Encoder : Filter
    {
        private static readonly Encoding UTF8 = Encoding.UTF8;

        public string getName()
        {
            return "utf8_encode";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            byte[] raw = encoding.GetBytes(content);
            byte[] rawUtf8 = Encoding.Convert(encoding, UTF8, raw);

            char[] utf8Chars = new char[UTF8.GetCharCount(rawUtf8, 0, rawUtf8.Length)];
            UTF8.GetChars(rawUtf8, 0, rawUtf8.Length, utf8Chars, 0);

            return new String(utf8Chars);
        }
    }

    public class Br2Nl : Filter
    {
        private static readonly Regex BR2NL_REGEX = new Regex("<br\\s*/?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string getName()
        {
            return "br2nl";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            return BR2NL_REGEX.Replace(content, "\n");
        }
    }

    public class P2Break : Filter
    {
        private static readonly Regex P2BREAK_REGEX = new Regex("<p[^/>]*/?>(\\s*</p>)?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public string getName()
        {
            return "p2break";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            return P2BREAK_REGEX.Replace(content, "\n");
        }
    }

    public class WhitespaceCleaner : Filter
    {
        private static readonly Regex CLEAN_WHITESPACE_REGEX = new Regex("^\\s+|\\s+$", RegexOptions.Compiled);
        private static readonly Regex CLEAN_LINES_REGEX = new Regex("\n{3,}", RegexOptions.Compiled);

        public string getName()
        {
            return "clean_spaces";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            content = content.Replace("\r\n", "\n").Replace('\r', '\n');
            content = CLEAN_WHITESPACE_REGEX.Replace(content, "\n\n");
            content = CLEAN_LINES_REGEX.Replace(content, "");

            return content;
        }
    }

    public class Trimmer : Filter
    {
        public string getName()
        {
            return "trim";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            return content.Trim();
        }
    }

    public class Lowercaser : Filter
    {
        public string getName()
        {
            return "lowercase";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            return content.ToLower(); // TODO culture info
        }
    }

    public class Uppercaser : Filter
    {
        public string getName()
        {
            return "uppercase";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            return content.ToUpper(); // TODO culture info
        }
    }

    public class DiacriticsNormalizer : Filter
    {
        public string getName()
        {
            return "uni2ascii";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            string normalized = content.Normalize(NormalizationForm.FormD);
            StringBuilder newString = new StringBuilder();

            foreach (char c in normalized)
            {
                if (char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    newString.Append(c);
                }
            }

            return newString.ToString();
        }
    }

    public class UrlEncoder : Filter
    {
        public string getName()
        {
            return "urlencode";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            return HttpUtility.UrlEncode(content, encoding);
        }
    }

    public class RegexReplacer : Filter
    {
        private readonly Dictionary<string, Regex> regexCache = new Dictionary<string, Regex>();
        private readonly object lockObject = new object();

        public string getName()
        {
            return "regex";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            if (args.Length < 2)
            {
                throw new Exception("The regex filter needs at least 2 arguments: regex:<pattern>,<replacement>[,<options>]");
            }

            string pattern = HttpUtility.UrlDecode(args[0], Encoding.ASCII);
            string replacement = HttpUtility.UrlDecode(args[0], Encoding.ASCII);
            string optionString = "";
            if (args.Length > 2)
            {
                optionString = args[2];
            }

            Regex regex;
            try
            {
                lock (this.lockObject)
                {
                    regex = this.regexCache[cacheKey(pattern, optionString)];
                }
            }
            catch
            {
                RegexOptions options = RegexOptions.Compiled | Pattern.optionStringToRegexOptions(optionString);
                regex = new Regex(pattern, options);
                lock (this.lockObject)
                {
                    this.regexCache.Add(cacheKey(pattern, optionString), regex);
                }
            }

            content = regex.Replace(content, replacement);

            return content;
        }

        private static string cacheKey(string pattern, string optionString)
        {
            return pattern + '|' + optionString;
        }
    }

    public class NonAsciiStripper : Filter
    {
        public string getName()
        {
            return "strip_nonascii";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            if (args.Length < 1)
            {
                throw new Exception("The strip_nonascii filter need at least 1 arg: strip_nonascii:<replacement>[,duplicate]");
            }
            string replacement = args[0];
            bool duplicate = false;
            if (args.Length > 1)
            {
                duplicate = true;
            }

            StringBuilder newString = new StringBuilder();

            char c;
            bool replacedLast = false;
            foreach (char cur in content)
            {
                c = char.ToLower(cur);
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    replacedLast = false;
                    newString.Append(cur);
                }
                else if (!replacedLast || duplicate)
                {
                    newString.Append(replacement);
                    replacedLast = true;
                }
            }

            return newString.ToString();
        }
    }
}
