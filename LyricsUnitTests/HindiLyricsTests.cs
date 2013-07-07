using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class HindiLyricsTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Hindi Lyrics");

        [Timeout(3000)]
        [TestMethod]
        public void hindiLyricsBasics()
        {
            String lyr = PROVIDER.getLyrics("", "Raanjhna Hua Main Tera", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
