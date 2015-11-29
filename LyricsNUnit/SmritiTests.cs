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
using NUnit.Framework;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestFixture]
	public class SmritiTests : BaseTest
    {
        [Timeout(3000)]
		[Test]
        public void smritiBasics()
        {
			String lyr = getProvider("Smriti").getLyrics("", "ba.Dii mushkil se huaa teraa meraa saath piyaa", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
