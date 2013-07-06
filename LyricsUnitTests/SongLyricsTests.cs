using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class SongLyricsTests
    {
        private static readonly Provider p = LyricsTests.getProvider("A-Z Lyrics Universe");

        [TestMethod]
        public void songLyricsBasics()
        {
            String lyr = p.getLyrics("die Ärzte", "Schrei nach Liebe", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
