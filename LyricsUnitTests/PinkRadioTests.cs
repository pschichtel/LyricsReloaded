using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class PinkRadioTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Pink Radio");

        [Timeout(3000)]
        [TestMethod]
        public void pinkRadioBasics()
        {
            String lyr = PROVIDER.getLyrics("", "Sa tvojih usana", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
