using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CubeIsland.LyricsReloaded;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsUnitTests
{
    public class LyricsTests
    {
        private static LyricsReloaded lr = null;

        public static LyricsReloaded getLyricsReloaded()
        {
            if (lr == null)
            {
                lr = new LyricsReloaded(".");
                lr.loadConfigurations();
            }
            return lr;
        }

        public static Provider getProvider(string name)
        {
            Provider p = getLyricsReloaded().getProviderManager().getProvider(name);
            Assert.IsNotNull(p, "Provider '" + name + "' not found!");
            return p;
        }
    }
}
