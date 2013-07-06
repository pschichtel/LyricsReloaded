using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class CuspajzTests
    {
        private static readonly Provider p = LyricsTests.getProvider("Cušpajz");

        [TestMethod]
        public void cuspajzBasics()
        {
            String lyr = p.getLyrics("Zabranjeno pušenje", "Kada dernek utihne", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}
