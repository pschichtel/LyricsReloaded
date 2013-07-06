﻿using System;
using System.Text;
using CubeIsland.LyricsReloaded;
using CubeIsland.LyricsReloaded.Provider;

namespace LyricsTester
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.Title = "LyricsReloaded!";
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Console.CancelKeyPress += (sender, eventArgs) => Console.WriteLine("Bye!");

            String providerName = null;
            String artist = null;
            String title = null;
            String album = null;

            int result = 0;

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

            LyricsReloaded lyricsReloaded = new LyricsReloaded(".");
            lyricsReloaded.loadConfigurations();

            if (String.IsNullOrWhiteSpace(providerName))
            {
                Console.WriteLine("The providers:");
                foreach (Provider p in lyricsReloaded.getProviderManager().getProviders())
                {
                    Console.WriteLine(" - {0}", p.getName());
                }
                Console.Write("Enter the provider: ");
                providerName = Console.ReadLine();
                if (providerName != null)
                {
                    providerName = providerName.Trim();
                }
            }
            if (String.IsNullOrWhiteSpace(artist))
            {
                Console.Write("Enter the artist: ");
                artist = Console.ReadLine();
                if (artist != null)
                {
                    artist = artist.Trim();
                }
            }
            if (String.IsNullOrWhiteSpace(title))
            {
                Console.Write("Enter the title: ");
                title = Console.ReadLine();
                if (title != null)
                {
                    title = title.Trim();
                }
            }


            Provider provider = lyricsReloaded.getProviderManager().getProvider(providerName);
            if (provider == null)
            {
                lyricsReloaded.getLogger().error("Provider {0} not found!", providerName);
                result = 1;
            }
            else
            {
                Console.Write("Provider {0}: ", providerName);
                try
                {
                    String lyrics = provider.getLyrics(artist, title, album);
                    if (String.IsNullOrWhiteSpace(lyrics))
                    {
                        Console.WriteLine("failed (not found)");
                        lyricsReloaded.getLogger().error("Lyrics not found!");
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
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();

            return result;
        }
    }
}
