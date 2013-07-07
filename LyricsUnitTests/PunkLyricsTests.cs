using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class PunkLyricsTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Punk Lyrics");

        [TestMethod]
        public void punkLyricsBasics()
        {
            String lyr = PROVIDER.getLyrics("Die Ärzte", "Erklärung", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
