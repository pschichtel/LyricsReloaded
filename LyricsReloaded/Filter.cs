using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CubeIsland.LyricsReloaded
{
    public interface Filter
    {
        string filter(string content, Encoding encoding);
    }

    public class HtmlStripper : Filter
    {
        private static readonly Regex STRIP_TAG_REGEX = new Regex("<[a-z]+(\\s+[a-z]+(=(\"[^\"]*\"|'[^']*'|[^\\s\"'/>]+))?)*\\s*/?>|</[a-z]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string filter(string content, Encoding encoding)
        {
            return STRIP_TAG_REGEX.Replace(content, "");
        }
    }

    public class HtmlEntityDecoder : Filter
    {
        public string filter(string content, Encoding encoding)
        {
            return HttpUtility.HtmlDecode(content);
        }
    }

    public class LinkRemover : Filter
    {
        private static readonly Regex STRIP_LINKS_REGEX = new Regex("https?://(\\s|$)*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string filter(string content, Encoding encoding)
        {
            return STRIP_LINKS_REGEX.Replace(content, "");
        }
    }

    public class UTF8Encoder : Filter
    {
        private static readonly Encoding UTF8 = Encoding.UTF8;

        public string filter(string content, Encoding encoding)
        {
            byte[] raw = encoding.GetBytes(content);
            byte[] rawUtf8 = Encoding.Convert(encoding, UTF8, raw);

            char[] utf8Chars = new char[UTF8.GetCharCount(rawUtf8, 0, rawUtf8.Length)];
            UTF8.GetChars(rawUtf8, 0, rawUtf8.Length, utf8Chars, 0);

            return new String(utf8Chars);
        }
    }
}
