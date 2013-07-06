﻿using System;
using CubeIsland.LyricsReloaded.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LyricsUnitTests
{
    [TestClass]
    public class LetrasMusBrTests
    {
        private static readonly Provider p = LyricsTests.getProvider("Letras de músicas");

        [TestMethod]
        public void letrasMusBrBasics()
        {
            String lyr = p.getLyrics("Mc Anitta", "Show Das Poderosas", "");

            Console.WriteLine(lyr);

            Assert.IsFalse(String.IsNullOrWhiteSpace(lyr), "Lyrics not found!");
        }
    }
}