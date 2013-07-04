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
        static int Main(string[] args)
        {
            String providerName = null;
            String artist = null;
            String title = null;
            String album = null;
            Boolean preferSync = false;

            int argc = args.Length;

            if (argc > 0)
            {
                providerName = args[0];
            }
            if (argc > 1)
            {
                artist = args[1];
            }
            if (argc > 2)
            {
                title = args[2];
            }
            if (argc > 3)
            {
                album = args[3];
            }
            if (argc > 4)
            {
                preferSync = true;
            }

            LyricsReloaded lyricsReloaded = new LyricsReloaded(".");

            if (String.IsNullOrWhiteSpace(providerName))
            {
                Console.WriteLine("The providers:");
                foreach (String name in lyricsReloaded.getProviderManager().getProviders().Keys)
                {
                    Console.WriteLine(" - " + name);
                }
                Console.Write("Enter the provider: ");
                providerName = Console.ReadLine();
            }
            if (String.IsNullOrWhiteSpace(artist))
            {
                Console.Write("Enter the artist: ");
                artist = Console.ReadLine();
            }
            if (String.IsNullOrWhiteSpace(title))
            {
                Console.Write("Enter the title: ");
                title = Console.ReadLine();
            }


            Provider provider = lyricsReloaded.getProviderManager().getProvider(providerName);
            if (provider == null)
            {
                Console.Error.WriteLine("Provider {0} not found!", providerName);
                return 1;
            }

            String lyrics;
            Console.Write("Provider " + providerName + ": ");
            try
            {
                lyrics = provider.getLyrics(artist, title, album);
                if (String.IsNullOrWhiteSpace(lyrics))
                {
                    Console.WriteLine("failed (not found)");
                }
                else
                {
                    Console.WriteLine("success\n\n" + lyrics);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("failed (internal error)");
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();

            return 0;
        }
    }
}
