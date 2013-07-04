using System;
using CubeIsland.LyricsReloaded;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsTester
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.Title = "LyricsReloaded!";
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs eventArgs) {
                Console.WriteLine("Bye!");
            };

            String providerName = null;
            String artist = null;
            String title = null;
            String album = null;
            Boolean preferSynced = false;

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
                preferSynced = true;
            }

            LyricsReloaded lyricsReloaded = new LyricsReloaded(".");
            lyricsReloaded.loadConfigurations();

            if (String.IsNullOrWhiteSpace(providerName))
            {
                Console.WriteLine("The providers:");
                foreach (Provider p in lyricsReloaded.getProviderManager().getProviders().Values)
                {
                    Console.WriteLine(" - {0}", p.getName());
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

            Console.Write("Provider " + providerName + ": ");
            try
            {
                String lyrics = provider.getLyrics(artist, title, album, preferSynced);
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
