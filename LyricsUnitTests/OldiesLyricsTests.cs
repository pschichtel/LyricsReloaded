using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class OldiesLyrics
    {
        private static readonly Provider p = LyricsTests.getProvider("Oldies Lyrics");

        [TestMethod]
        public void oldiesLyricsBasics()
        {
            String lyr = p.getLyrics("RODNEY CROWELL", "Why Don't We Talk About It", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
