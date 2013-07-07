using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class TekstowoTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Tekstowo");

        [Timeout(3000)]
        [TestMethod]
        public void tekstowoBasics()
        {
            String lyr = PROVIDER.getLyrics("Daniel Olbrychski", "Wyrzeźbiłem twoją twarz", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
