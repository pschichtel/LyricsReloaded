using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class SmritiTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Smriti");

        [TestMethod]
        public void smritiBasics()
        {
            String lyr = PROVIDER.getLyrics("", "ba.Dii mushkil se huaa teraa meraa saath piyaa", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
