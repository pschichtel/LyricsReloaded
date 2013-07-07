using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class SongLyricsTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Song Lyrics");

        [Timeout(3000)]
        [TestMethod]
        public void songLyricsBasics()
        {
            String lyr = PROVIDER.getLyrics("die Ärzte", "Schrei nach Liebe", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
