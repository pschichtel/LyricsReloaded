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

using YamlDotNet.RepresentationModel;
using System.Collections.Generic;
using System.Text;

namespace CubeIsland.LyricsReloaded.Provider.Loader
{
    public interface LyricsLoader
    {
        Lyrics getLyrics(Provider provider, Dictionary<string, string> variables);
    }

    public interface LyricsLoaderFactory
    {
        string getName();
        LyricsLoader newLoader(YamlMappingNode configuration);
    }

    public class Lyrics
    {
        private readonly string content;
        private readonly Encoding encoding;

        public Lyrics(string content, Encoding encoding)
        {
            this.content = content;
            this.encoding = encoding;
        }

        public string getContent()
        {
            return content;
        }

        public Encoding getEncoding()
        {
            return encoding;
        }
    }
}
