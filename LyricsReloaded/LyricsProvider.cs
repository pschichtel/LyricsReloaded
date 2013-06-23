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
using System.Collections.Specialized;
using System.Text;
using System.Web;
using YamlDotNet.RepresentationModel;
using System.Text.RegularExpressions;
using System.Globalization;
using MusicBeePlugin;
using CubeIsland.LyricsReloaded.Filters;

namespace CubeIsland.LyricsReloaded
{
    public interface LyricsProvider
    {
        string getName();
        string getLyrics(string artist, string title, string album);
    }
}
