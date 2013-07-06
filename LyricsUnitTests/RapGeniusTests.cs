using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class RapGeniusTests
    {
        private static readonly Provider p = LyricsTests.getProvider("Rap Genius");

        [TestMethod]
        public void rapGeniusBasics()
        {
            String lyr = p.getLyrics("Nesli", "Niente Di Più", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
