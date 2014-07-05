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
using System.Threading;
using CubeIsland.LyricsReloaded.Provider;
using CubeIsland.LyricsReloaded.Provider.Loader;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CubeIsland.LyricsReloaded
{
    public class LyricsReloaded
    {
        public static class FolderNames
        {
            public const string PROVIDERS = "providers";
            public const string DATA = "data";
        }

        private readonly string name;
        private readonly string dataFolder;
        private readonly Logger logger;
        private readonly ProviderManager providerManager;
        private string defaultUserAgent;
        private WebProxy proxy;
        private bool down = false;

        public LyricsReloaded(string configurationPath)
        {
            Assembly asm = Assembly.GetAssembly(GetType());
            name = asm.GetName().Name;
            dataFolder = Path.Combine(configurationPath, name);
            Directory.CreateDirectory(dataFolder);
            logger = new Logger(Path.Combine(dataFolder, name + ".log"));

            providerManager = new ProviderManager(this);

            providerManager.registerLoaderFactory(new StaticLoaderFactory(this));

            defaultUserAgent = "Firefox";
            proxy = null;
        }

        ~LyricsReloaded()
        {
            shutdown();
        }

        public Logger getLogger()
        {
            return logger;
        }

        public ProviderManager getProviderManager()
        {
            return providerManager;
        }

        public void setDefaultUserAgent(string userAgent)
        {
            defaultUserAgent = userAgent;
        }

        public string getDefaultUserAgent()
        {
            return defaultUserAgent;
        }

        public void setProxy(WebProxy newProxy)
        {
            proxy = newProxy;
        }

        public WebProxy getProxy()
        {
            return proxy;
        }

        public void shutdown()
        {
            if (!down)
            {
                down = true;
                providerManager.shutdown();
                logger.close();
            }
        }

        public void uninstall()
        {
            DirectoryInfo di = new DirectoryInfo(Path.Combine(dataFolder, FolderNames.PROVIDERS));

            if (di.GetFiles("*.yml").Length <= 0)
            {
                try
                {
                    logger.debug("Removing the providers folder...");
                    di.Delete(true);
                }
                catch (Exception e)
                {
                    logger.warn("Failed to remove provider folder: {0}", e.Message);
                }                  
            }

            try
            {
                logger.debug("Removing the data folder...");
                (new DirectoryInfo(Path.Combine(dataFolder, FolderNames.DATA))).Delete(true);
            }
            catch (Exception e)
            {
                logger.warn("Failed to remove provider folder: {0}", e.Message);
            }
        }

        public delegate void UpdateCheckerCallback(bool updateAvailable);

        public void checkForNewVersion(UpdateCheckerCallback callback)
        {
            Version local = Assembly.GetAssembly(GetType()).GetName().Version;

            LyricsReloaded lr = this;

            Thread updateChecker = new Thread(() => {
                WebClient cl = new WebClient(lr, 5000);
                try
                {
                    bool result = false;
                    WebResponse respone = cl.get("https://raw.github.com/quickwango/LyricsReloaded/stable/LyricsReloaded/Properties/AssemblyInfo.cs");
                    if (respone != null)
                    {
                        String content = respone.getContent();
                        if (!String.IsNullOrWhiteSpace(content))
                        {
                            Regex versionRegex = new Regex("AssemblyVersion\\(\"(?<version>[^\\s]+)\"\\)", RegexOptions.Compiled | RegexOptions.Singleline);
                            Match match = versionRegex.Match(content);
                            if (match.Success)
                            {
                                Version remote = Version.Parse(match.Groups["version"].Value.Replace("*", "0.0")); // TODO remove the replace() with the next release
                                result = remote.CompareTo(local) > 0;
                            }
                        }

                    }

                    callback(result);
                }
                catch (Exception e)
                {
                    lr.logger.error("Failed to check for updates: {0}", e.Message);
                }
            }) {
                IsBackground = true,
                Name = "LyricsReloaded - Version Check"
            };
            updateChecker.Start();

        }

        #region "internal helpers"

        public void loadConfigurations()
        {
            loadDefaultConfiguration();

            DirectoryInfo di = new DirectoryInfo(Path.Combine(dataFolder, FolderNames.PROVIDERS));
            if (!di.Exists)
            {
                try
                {
                    di.Create();
                }
                catch (IOException e)
                {
                    logger.warn("Failed to create the providers folder: {0}", e.Message);
                }
            }

            foreach (FileInfo fi in di.GetFiles("*.yml", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    providerManager.loadProvider(fi);
                }
                catch (InvalidConfigurationException e)
                {
                    logger.error("Failed to load a configuration:");
                    logger.error(e.Message);
                    if (e.InnerException != null)
                    {
                        logger.error(e.InnerException.ToString());
                    }
                }
            }
        }

        private void loadDefaultConfiguration()
        {
            foreach (PropertyInfo propInfo in typeof (Properties.Resources).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (!propInfo.Name.StartsWith("provider_", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                object value = propInfo.GetValue(null, null);
                if (value is String)
                {
                    logger.debug("Loading config from field {0}", propInfo.Name);
                    providerManager.loadProvider(value as String);
                }
            }
        }

        #endregion
    }
}
