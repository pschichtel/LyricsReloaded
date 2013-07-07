using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class AzLyricsTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("A-Z Lyrics Universe");

        [Timeout(3000)]
        [TestMethod]
        public void azLyricsBasics()
        {
            String lyr = PROVIDER.getLyrics("Skillet", "I Can", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
