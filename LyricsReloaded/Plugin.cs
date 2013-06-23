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

using System;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;
using CubeIsland.LyricsReloaded;
using CubeIsland.LyricsReloaded.Filters;
using System.Reflection;
using YamlDotNet.RepresentationModel;
using System.Resources;

namespace MusicBeePlugin
{
    public partial class Plugin
    {

        private MusicBeeApiInterface musicBee;
        private PluginInfo info = new PluginInfo();
        private LyricsReloaded lyricsReloaded;

        private Logger logger;
        private bool initialized = false;

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
                this.lyricsReloaded = new LyricsReloaded(this.musicBee);
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
                MessageBox.Show("An error occurred during plugin startup, send this file to the developer:\n\n" + this.logger.getFileInfo().FullName);
                this.logger.error(e.Message);
                throw e;
            }

            return this.info;
        }

        public void ReceiveNotification(String source, NotificationType type)
        {
            //MessageBox.Show("ReceiveNotification(" + source + ", " + type + ")");
            this.logger.debug("Received a notification of type {0}", type);
            switch (type)
            {
                case NotificationType.PluginStartup:
                    String proxySetting = this.musicBee.Setting_GetWebProxy();
                    if (!string.IsNullOrEmpty(proxySetting))
                    {
                        this.logger.debug("Proxy setting found");
                        string[] raw = proxySetting.Split(Convert.ToChar(0));
                        WebProxy proxy = new WebProxy(raw[0]);
                        if (raw.Length >= 3)
                        {
                            this.logger.debug("Proxy credentials found");
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
            this.logger.info("Closing ...");
            this.initialized = false;
        }

        public void init()
        {
            if (!this.initialized)
            {
                this.initialized = true;
                this.lyricsReloaded.getLogger().info("Plugin initialized!");
            }
        }

        public String[] GetProviders()
        {
            Dictionary<string, LyricsProvider> providers = this.lyricsReloaded.getProviders();
            string[] providerNames = new string[providers.Count];
            providers.Keys.CopyTo(providerNames, 0);
            return providerNames;
        }

        public String RetrieveLyrics(String source, String artist, String title, String album, bool preferSynced, String providerName)
        {
            this.logger.debug("Lyrics request: {0} - {1} - {2} - {3} - {4} - {5}", source, artist, title, album, (preferSynced ? "synced" : "unsynced"), providerName);
            LyricsProvider provider = this.lyricsReloaded.getProvider(providerName);
            if (provider == null)
            {
                this.lyricsReloaded.getLogger().warn("The provider {0} was not found!", providerName);
                return null;
            }

            string lyricsContent = provider.getLyrics(artist, title, album);

            // this.logger.debug("{0} constructed this URL: {1}", provider.getName(), url);

            // LyricsResponse response = this.loader.loadContent(url, "USER_AGENT");

            // String content = provider.processContent(response.getContent(), response.getEncoding());

            // if (String.IsNullOrWhiteSpace(content))
            // {
            //     this.logger.debug("no lyrics found");
            //     return null;
            // }

            // this.logger.debug("lyrics found");

            // return content;
            return null;
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

        // used for testing only
        public static IntPtr mockApi()
        {
            MusicBeeApiInterface api = new Plugin.MusicBeeApiInterface();

            api.Setting_GetWebProxy = delegate() {
                return "";
            };

            api.Setting_GetPersistentStoragePath = delegate() {
                return Path.GetDirectoryName(Assembly.GetAssembly(typeof(LyricsLoader)).Location);
            };

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(api));
            Marshal.StructureToPtr(api, ptr, true);

            return ptr;
        }
    }
}
