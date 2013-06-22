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
using System.Web;
using YamlDotNet.RepresentationModel;
using System.Text.RegularExpressions;
using System.Globalization;
using MusicBeePlugin;
using CubeIsland.LyricsReloaded.Filters;

namespace CubeIsland.LyricsReloaded
{
    public class LyricsProvider
    {
        private readonly Plugin plugin;
        private readonly string name;
        private readonly string url;
        private readonly LinkedList<Expression> expressions;
        private readonly LinkedList<Filter> filters;

        public LyricsProvider(Plugin plugin, string name, string url, LinkedList<Expression> expressions, LinkedList<Filter> filters)
        {
            this.plugin = plugin;
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
            return new UrlCompiler(this.url, artist, title, album).compile();
        }

        public String processContent(string content, Encoding encoding)
        {
            foreach (Expression expr in this.expressions)
            {
                try
                {
                    content = expr.apply(content);
                }
                catch (Exception e)
                {
                    this.plugin.getLogger().warn(e.Message);
                    return null;
                }
            }
            foreach (Filter filter in this.filters)
            {
                try
                {
                    content = filter.filter(content, encoding);
                }
                catch (Exception e)
                {
                    this.plugin.getLogger().warn("The filter {0} failed: {1}", filter.getName(), e.Message);
                }
            }
            return content;
        }

        private class UrlCompiler
        {
            private static readonly Regex REPLACE_ARTIST_REGEX = new Regex("{artist(?::([a-z,]+))}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            private static readonly Regex REPLACE_TITLE_REGEX = new Regex("{title(?::([a-z,]+))}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            private static readonly Regex REPLACE_ALBUM_REGEX = new Regex("{album(?::([a-z,]+))}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            private readonly string url;
            private readonly String artist;
            private readonly String title;
            private readonly String album;

            public UrlCompiler(string url, String artist, String title, String album)
            {
                this.url = url;
                this.artist = artist;
                this.title = title;
                this.album = album;
            }

            public string compile()
            {
                string url = this.url;
                if (this.artist != null)
                {
                    url = REPLACE_ARTIST_REGEX.Replace(url, this.insertArtist);
                }
                if (this.title != null)
                {
                    url = REPLACE_TITLE_REGEX.Replace(url, this.insertTitle);
                }
                if (this.album != null)
                {
                    url = REPLACE_ALBUM_REGEX.Replace(url, this.insertAlbum);
                }
                return url;
            }

            private String applyFilters(string content, String argString)
            {
                if (argString == null)
                {
                    return content;
                }
                string[] args = argString.Split(',');

                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "lower":
                            content = content.ToLower();
                            break;
                        case "upper":
                            content = content.ToUpper();
                            break;
                        case "entity":
                            content = HttpUtility.UrlEncode(content, Encoding.UTF8);
                            break;
                        case "alpha":
                            content = stripDown(content);
                            break;
                        case "name":
                            content = stripDown(content, "-");
                            break;
                        case "uscore":
                            content = stripDown(content, "_");
                            break;
                        case "normal":
                            content = normaizeString(content);
                            break;
                    }
                }

                return content;
            }

            private static string stripDown(string content)
            {
                return stripDown(content, "");
            }

            private static string stripDown(string content, string replacement)
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
                    else if (!replacedLast)
                    {
                        newString.Append(replacement);
                        replacedLast = true;
                    }
                }

                return newString.ToString();
            }

            private static string normaizeString(string content)
            {
                try
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
                catch
                {
                    return content;
                }
            }

            private String insertArtist(Match match)
            {
                return this.applyFilters(this.artist, match.Groups[1].ToString());
            }

            private String insertTitle(Match match)
            {
                return this.applyFilters(this.title, match.Groups[1].ToString());
            }

            private String insertAlbum(Match match)
            {
                return this.applyFilters(this.album, match.Groups[1].ToString());
            }
        }
    }
}
