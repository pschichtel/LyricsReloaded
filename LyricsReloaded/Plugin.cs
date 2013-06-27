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
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
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
            this.musicBee = new MusicBeeApiInterface();
            this.musicBee.Initialise(apiPtr);

            this.info.PluginInfoVersion = PluginInfoVersion;
            this.info.Name = "Lyrics Reloaded!";
            this.info.Description = "Lyrics loading done properly!";
            this.info.Author = "Phillip Schichtel <Quick_Wango>";
            this.info.TargetApplication = "MusicBee";
            this.info.Type = PluginType.LyricsRetrieval;
            this.info.VersionMajor = 1;
            this.info.VersionMinor = 0;
            this.info.Revision = 1;
            this.info.MinInterfaceVersion = 20;
            this.info.MinApiRevision = 25;
            this.info.ReceiveNotifications = ReceiveNotificationFlags.StartupOnly;
            this.info.ConfigurationPanelHeight = 0;

            try
            {
                this.lyricsReloaded = new LyricsReloaded(this.musicBee.Setting_GetPersistentStoragePath());
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred during plugin startup: " + e.Message);
                throw e;
            }

            try
            {
                this.lyricsReloaded.loadConfigurations();
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred during plugin startup, send this file to the developer:\n\n" + this.lyricsReloaded.getLogger().getFileInfo().FullName);
                this.lyricsReloaded.getLogger().error(e.Message);
                throw e;
            }

            return this.info;
        }

        public void ReceiveNotification(String source, NotificationType type)
        {
            //MessageBox.Show("ReceiveNotification(" + source + ", " + type + ")");
            this.lyricsReloaded.getLogger().debug("Received a notification of type {0}", type);
            switch (type)
            {
                case NotificationType.PluginStartup:
                    String proxySetting = this.musicBee.Setting_GetWebProxy();
                    if (!string.IsNullOrEmpty(proxySetting))
                    {
                        this.lyricsReloaded.getLogger().debug("Proxy setting found");
                        string[] raw = proxySetting.Split(Convert.ToChar(0));
                        WebProxy proxy = new WebProxy(raw[0]);
                        if (raw.Length >= 3)
                        {
                            this.lyricsReloaded.getLogger().debug("Proxy credentials found");
                            proxy.Credentials = new NetworkCredential(raw[1], raw[2]);
                        }
                        this.lyricsReloaded.setProxy(proxy);
                    }

                    break;
            }
        }

        public void Close(PluginCloseReason reason)
        {
            //MessageBox.Show("Close(" + reason + ")");
            this.lyricsReloaded.getLogger().info("Plugin disabled");
        }

        public String[] GetProviders()
        {
            Dictionary<string, Provider> providers = this.lyricsReloaded.getProviderManager().getProviders();
            string[] providerNames = new string[providers.Count];
            providers.Keys.CopyTo(providerNames, 0);
            return providerNames;
        }

        public String RetrieveLyrics(String source, String artist, String title, String album, bool preferSynced, String providerName)
        {
            this.lyricsReloaded.getLogger().debug("Lyrics request: {0} - {1} - {2} - {3} - {4} - {5}", source, artist, title, album, (preferSynced ? "synced" : "unsynced"), providerName);
            Provider provider = this.lyricsReloaded.getProviderManager().getProvider(providerName);
            if (provider == null)
            {
                this.lyricsReloaded.getLogger().warn("The provider {0} was not found!", providerName);
                return null;
            }

            String lyrics = provider.getLyrics(artist, title, album);

            if (String.IsNullOrWhiteSpace(lyrics))
            {
                this.lyricsReloaded.getLogger().debug("no lyrics found");
                return null;
            }

            this.lyricsReloaded.getLogger().debug("lyrics found");

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
