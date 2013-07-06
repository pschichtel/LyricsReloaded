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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CubeIsland.LyricsReloaded.Provider;

namespace CubeIsland.LyricsReloaded.Filters
{
    public interface Filter
    {
        string getName();
        string filter(string content, string[] args, Encoding encoding);
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

    public class Utf8Encoder : Filter
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
        private static readonly Regex CLEAN_LINES_REGEX = new Regex("\n{3,}", RegexOptions.Compiled);
        private static readonly Regex CLEAN_SPACES_REGEX = new Regex(" {2,}", RegexOptions.Compiled);

        public string getName()
        {
            return "clean_spaces";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            // tab -> space, v-tab -> newline
            content = content.Replace("\t", " ").Replace("\v", "\n");

            // normalize newlines to unix-style
            content = content.Replace("\r\n", "\n").Replace('\r', '\n');

            // strip unnecessary newlines
            content = CLEAN_LINES_REGEX.Replace(content, "\n\n");

            // strip unnecessary spaces
            content = CLEAN_SPACES_REGEX.Replace(content, " ");

            // trim and return
            return content.Trim();
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
            return "diacritics2ascii";
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

    public class Umlauts2Ascii : Filter
    {
        private static readonly Dictionary<string, string> UMLAUT_MAP = new Dictionary<string, string> {
            {"ä", "ae"},
            {"ö", "oe"},
            {"ü", "ue"},
            {"ß", "ss"}
        };

        public string getName()
        {
            return "umlauts2ascii";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            foreach (KeyValuePair<string, string> entry in UMLAUT_MAP)
            {
                content = content.Replace(entry.Key, entry.Value);
            }
            return content;
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

    public class UriEscaper : Filter
    {
        public string getName()
        {
            return "uriescape";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            return Uri.EscapeUriString(content);
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
                throw new InvalidConfigurationException("The regex filter needs at least 2 arguments: regex, <pattern>, <replacement>[, <options>]");
            }

            string pattern = args[0];
            string replacement = args[1];
            string optionString = "";
            if (args.Length > 2)
            {
                optionString = args[2];
            }

            Regex regex;
            try
            {
                lock (lockObject)
                {
                    regex = regexCache[cacheKey(pattern, optionString)];
                }
            }
            catch
            {
                RegexOptions options = RegexOptions.Compiled | Pattern.regexOptionsFromString(optionString);
                regex = new Regex(pattern, options);
                lock (lockObject)
                {
                    regexCache.Add(cacheKey(pattern, optionString), regex);
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
            string replacement = "";
            if (args.Length > 0)
            {
                replacement = args[0];
            }
            return strip(content, replacement, args.Length > 1);
        }

        public static string strip(string content, string replacement = "", bool duplicate = false)
        {
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

    public class ReplaceFilter : Filter
    {
        public string getName()
        {
            return "replace";
        }

        public string filter(string content, string[] args, Encoding encoding)
        {
            if (args.Length < 2)
            {
                throw new InvalidConfigurationException("The replace filter requires 2 parameters: replace, <search>, <replacement>");
            }
            return content.Replace(args[0], args[1]);
        }
    }
}
