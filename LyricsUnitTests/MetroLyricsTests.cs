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
