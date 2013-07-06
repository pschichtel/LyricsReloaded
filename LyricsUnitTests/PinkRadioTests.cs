using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class PinkRadioTests
    {
        private static readonly Provider p = LyricsTests.getProvider("Pink Radio");

        [TestMethod]
        public void pinkRadioBasics()
        {
            String lyr = p.getLyrics("", "Sa tvojih usana", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
