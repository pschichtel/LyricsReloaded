using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicBeePlugin;
using System.Reflection;
using System.Runtime.InteropServices;
using CubeIsland.LyricsReloaded;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    [TestClass]
    public class LyricsTests
    {
        private static LyricsReloaded lyricsReloaded = null;

        [ClassInitialize]
        public static void init(TestContext context)
        {
            lyricsReloaded = new LyricsReloaded(".");
        }

        [ClassCleanup]
        public static void cleanup()
        {
            lyricsReloaded = null;
        }

        [TestMethod]
        public void testEnabling()
        {
        }
    }
}
