using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicBeePlugin;

namespace LyricsTester
{
    class Program
    {
        static void Main(string[] args)
        {
            MusicBeePlugin.Plugin plugin = new Plugin();

            IntPtr structPointer = Plugin.mockApi();

            plugin.Initialise(structPointer);
            plugin.ReceiveNotification("tester", Plugin.NotificationType.PluginStartup);

            Console.WriteLine("The providers:");
            foreach (String provider in plugin.GetProviders())
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
            foreach (String provider in plugin.GetProviders())
            {
                Console.Write("Provider " + provider + ": ");
                try
                {
                    lyrics = plugin.RetrieveLyrics("abc.mp3", artist, title, "", false, provider);
                    if (string.IsNullOrWhiteSpace(lyrics))
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

            plugin.Close(Plugin.PluginCloseReason.MusicBeeClosing);
            Marshal.FreeHGlobal(structPointer);
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
