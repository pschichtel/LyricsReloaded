using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class RapGeniusTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Rap Genius");

        [TestMethod]
        public void rapGeniusBasics()
        {
            String lyr = PROVIDER.getLyrics("Nesli", "Niente Di Più", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
