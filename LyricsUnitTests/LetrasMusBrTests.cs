using System;
using CubeIsland.LyricsReloaded.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LyricsUnitTests
{
    [TestClass]
    public class LetrasMusBrTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Letras de músicas");

        [Timeout(3000)]
        [TestMethod]
        public void letrasMusBrBasics()
        {
            String lyr = PROVIDER.getLyrics("Mc Anitta", "Show Das Poderosas", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
