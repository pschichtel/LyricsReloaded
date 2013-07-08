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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class MetroLyricsTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("MetroLyrics");

        [Timeout(3000)]
        [TestMethod]
        public void metroLyricsBasics()
        {
            String lyr = PROVIDER.getLyrics("Lil Wayne", "We Be Steady Mobbin''", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }

        [Timeout(3000)]
        [TestMethod]
        public void metroLyricsNonAsciiSpaceEdgeCase()
        {
            String lyr = PROVIDER.getLyrics("Lil Wayne", "Mr. Carter", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
