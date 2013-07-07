using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class UrbanLyricsTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Urban Lyrics");

        [Timeout(3000)]
        [TestMethod]
        public void urbanLyricsBasics()
        {
            String lyr = PROVIDER.getLyrics("50 Cent", "Buzzin' (Remix)", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
