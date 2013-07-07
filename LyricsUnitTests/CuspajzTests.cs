using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class CuspajzTests
    {
        private static readonly Provider PROVIDER = LyricsTests.getProvider("Cušpajz");

        [TestMethod]
        public void cuspajzBasics()
        {
            String lyr = PROVIDER.getLyrics("Zabranjeno pušenje", "Kada dernek utihne", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
