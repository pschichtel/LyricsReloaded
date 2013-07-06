using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class AzLyricsTests
    {
        private static readonly Provider p = LyricsTests.getProvider("A-Z Lyrics Universe");

        [TestMethod]
        public void azLyricsBasics()
        {
            String lyr = p.getLyrics("Skillet", "I Can", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
