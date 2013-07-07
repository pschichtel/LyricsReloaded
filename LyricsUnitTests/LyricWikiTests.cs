using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class LyricWikiTests
    {
        private static readonly Provider NORMAL = LyricsTests.getProvider("LyricWiki");
        private static readonly Provider GRACENOTE = LyricsTests.getProvider("LyricWiki Gracenote");

        [Timeout(3000)]
        [TestMethod]
        public void lyricWikiNormalBasics()
        {
            String lyr = NORMAL.getLyrics("Paramore", "Misery Business", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }

        [Timeout(3000)]
        [TestMethod]
        public void lyricWikiGracenoteBasics()
        {
            String lyr = GRACENOTE.getLyrics("Paramore", "Misery Business", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
