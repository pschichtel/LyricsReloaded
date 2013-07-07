using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class TekstoviPjesamaTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Tekstovi Pjesama");

        [Timeout(3000)]
        [TestMethod]
        public void cuspajzBasics()
        {
            String lyr = PROVIDER.getLyrics("Šaban Šaulić", "Koliko ti srece zelim", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
