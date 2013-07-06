using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class LyricWikiTests
    {
        private static readonly Provider normal = LyricsTests.getProvider("LyricWiki");
        private static readonly Provider gracenote = LyricsTests.getProvider("LyricWiki Gracenote");

        [TestMethod]
        public void lyricWikiNormalBasics()
        {
            String lyr = normal.getLyrics("Paramore", "Misery Business", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }

        [TestMethod]
        public void lyricWikiGracenoteBasics()
        {
            String lyr = gracenote.getLyrics("Paramore", "Misery Business", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
