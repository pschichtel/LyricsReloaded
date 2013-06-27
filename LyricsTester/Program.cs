using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CubeIsland.LyricsReloaded;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsTester
{
    class Program
    {
        static void Main(string[] args)
        {
            LyricsReloaded lyricsReloaded = new LyricsReloaded(".");

            Console.WriteLine("The providers:");
            foreach (String provider in lyricsReloaded.getProviderManager().getProviders().Keys)
            {
                Console.WriteLine(" - " + provider);
            }

            Console.Write("Enter artist: ");
            string artist = def(Console.ReadLine(), "Paramore");

            Console.Write("Enter title: ");
            string title = def(Console.ReadLine(), "Misery Business");

            //Console.Write("Enter album: ");
            //string album = def(Console.ReadLine(), "Roit!");

            String lyrics;
            foreach (String providerName in lyricsReloaded.getProviderManager().getProviders().Keys)
            {
                Console.Write("Provider " + providerName + ": ");
                try
                {
                    Provider provider = lyricsReloaded.getProviderManager().getProvider(providerName);
                    lyrics = provider.getLyrics(artist, title, "");
                    if (String.IsNullOrWhiteSpace(lyrics))
                    {
                        Console.WriteLine("failed (not found)");
                    }
                    else
                    {
                        Console.WriteLine("success\n\n" + lyrics);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("failed (" + e.Message + ")");
                }
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static string def(String str, string value)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return value;
            }
            return str;
        }
    }
}
