using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class PunkLyricsTests
    {
        private static readonly Provider p = LyricsTests.getProvider("Punk Lyrics");

        [TestMethod]
        public void punkLyricsBasics()
        {
            String lyr = p.getLyrics("Die Ärzte", "Erklärung", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
