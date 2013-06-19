using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicBeePlugin;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LyricsUnitTests
{
    [TestClass]
    public class LyricsTests
    {
        private Plugin plugin;
        private IntPtr api;

        [TestInitialize]
        public void init()
        {
            plugin = new Plugin();
            api = Plugin.mockApi();
        }

        [TestCleanup]
        public void cleanup()
        {
            plugin = null;
            Marshal.FreeHGlobal(api);
        }

        private void startup()
        {
            this.plugin.Initialise(this.api);
            this.plugin.ReceiveNotification("test", Plugin.NotificationType.PluginStartup);
        }

        private void shutdown()
        {
            this.plugin.Close(Plugin.PluginCloseReason.MusicBeeClosing);
        }

        [TestMethod]
        public void testEnabling()
        {
            this.startup();

            plugin.Close(Plugin.PluginCloseReason.UserDisabled);

            plugin.ReceiveNotification("test", Plugin.NotificationType.PluginStartup);

            plugin.Close(Plugin.PluginCloseReason.UserDisabled);

            plugin.ReceiveNotification("test", Plugin.NotificationType.PluginStartup);

            plugin.Close(Plugin.PluginCloseReason.UserDisabled);

            plugin.ReceiveNotification("test", Plugin.NotificationType.PluginStartup);

            this.shutdown();
        }

        [TestMethod]
        public void testBuildInProviders()
        {
            this.startup();

            Assert.AreEqual(10, plugin.GetProviders().Length);

            this.shutdown();
        }

        [TestMethod]
        public void testSimpleSearch()
        {
            this.startup();

            String lyrics = null;
            foreach (String provider in plugin.GetProviders())
            {
                lyrics = plugin.RetrieveLyrics("", "Paramore", "Misery Business", "Roit!", false, provider);
                if (lyrics != null)
                {
                    break;
                }
            }

            Assert.IsNotNull(lyrics);

            this.shutdown();
        }
    }
}
