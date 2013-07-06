using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class MetroLyricsTests
    {
        private static readonly Provider p = LyricsTests.getProvider("MetroLyrics");

        [TestMethod]
        public void metroLyricsBasics()
        {
            String lyr = p.getLyrics("Lil Wayne", "We Be Steady Mobbin''", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }

        [TestMethod]
        public void metroLyricsNonAsciiSpaceEdgeCase()
        {
            String lyr = p.getLyrics("Lil Wayne", "Mr. Carter", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
