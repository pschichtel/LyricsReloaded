/*
    Copyright 2013 Phillip Schichtel

    This file is part of LyricsReloaded.

    LyricsReloaded is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    LyricsReloaded is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with LyricsReloaded. If not, see <http://www.gnu.org/licenses/>.

*/

using CubeIsland.LyricsReloaded;
using CubeIsland.LyricsReloaded.Provider;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class Plugin
    {
        private MusicBeeApiInterface musicBee;
        private PluginInfo info = new PluginInfo();
        private LyricsReloaded lyricsReloaded;

        // Called from MusicBee
        public PluginInfo Initialise(IntPtr apiPtr)
        {
            //MessageBox.Show("Initialised(" + apiPtr + ")");
            musicBee = new MusicBeeApiInterface();
            musicBee.Initialise(apiPtr);

            info.PluginInfoVersion = PluginInfoVersion;
            info.Name = "Lyrics Reloaded!";
            info.Description = "Lyrics loading done properly!";
            info.Author = "Phillip Schichtel <Quick_Wango>";
            info.TargetApplication = "MusicBee";
            info.Type = PluginType.LyricsRetrieval;
            info.VersionMajor = 1;
            info.VersionMinor = 0;
            info.Revision = 1;
            info.MinInterfaceVersion = 20;
            info.MinApiRevision = 25;
            info.ReceiveNotifications = ReceiveNotificationFlags.StartupOnly;
            info.ConfigurationPanelHeight = 0;

            try
            {
                lyricsReloaded = new LyricsReloaded(musicBee.Setting_GetPersistentStoragePath());
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred during plugin startup: " + e.Message);
                throw;
            }

            try
            {
                lyricsReloaded.loadConfigurations();
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred during plugin startup, send this file to the developer:\n\n" +
                                lyricsReloaded.getLogger().getFileInfo().FullName);
                lyricsReloaded.getLogger().error(e.Message);
                throw;
            }

            return info;
        }

        public void ReceiveNotification(String source, NotificationType type)
        {
            //MessageBox.Show("ReceiveNotification(" + source + ", " + type + ")");
            lyricsReloaded.getLogger().debug("Received a notification of type {0}", type);
            switch (type)
            {
                case NotificationType.PluginStartup:
                    String proxySetting = musicBee.Setting_GetWebProxy();
                    if (!string.IsNullOrEmpty(proxySetting))
                    {
                        lyricsReloaded.getLogger().debug("Proxy setting found");
                        string[] raw = proxySetting.Split(Convert.ToChar(0));
                        WebProxy proxy = new WebProxy(raw[0]);
                        if (raw.Length >= 3)
                        {
                            lyricsReloaded.getLogger().debug("Proxy credentials found");
                            proxy.Credentials = new NetworkCredential(raw[1], raw[2]);
                        }
                        lyricsReloaded.setProxy(proxy);
                    }

                    break;
            }
        }

        public void Close(PluginCloseReason reason)
        {
            //MessageBox.Show("Close(" + reason + ")");
            lyricsReloaded.getLogger().info("Plugin disabled");
            lyricsReloaded.getProviderManager().clean();
            lyricsReloaded = null;
        }

        public String[] GetProviders()
        {
            Dictionary<string, Provider> providers = lyricsReloaded.getProviderManager().getProviders();
            string[] providerNames = new string[providers.Count];
            providers.Keys.CopyTo(providerNames, 0);
            return providerNames;
        }

        public String RetrieveLyrics(String source, String artist, String title, String album, bool preferSynced, String providerName)
        {
            lyricsReloaded.getLogger().debug("Lyrics request: {0} - {1} - {2} - {3} - {4}", source, artist, title, album, providerName);
            Provider provider = lyricsReloaded.getProviderManager().getProvider(providerName);
            if (provider == null)
            {
                lyricsReloaded.getLogger().warn("The provider {0} was not found!", providerName);
                return null;
            }

            String lyrics = provider.getLyrics(artist, title, album);

            if (String.IsNullOrWhiteSpace(lyrics))
            {
                lyricsReloaded.getLogger().debug("no lyrics found");
                return null;
            }

            lyricsReloaded.getLogger().debug("lyrics found");

            return lyrics;
        }

        #region "MusicBee implementations"
        public bool Configure(IntPtr panelHandle)
        {
            return true;
        }

        public void SaveSettings()
        {}

        public void Uninstall()
        {}

        #endregion
    }
}
