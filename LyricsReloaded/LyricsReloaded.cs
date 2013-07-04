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
using CubeIsland.LyricsReloaded.Provider;
using CubeIsland.LyricsReloaded.Provider.Loader;
using System.IO;
using System.Net;
using System.Reflection;

namespace CubeIsland.LyricsReloaded
{
    public class LyricsReloaded
    {
        private readonly string name;
        private readonly string pluginDirectory;
        private readonly Logger logger;
        private readonly ProviderManager providerManager;
        private string userAgent;
        private WebProxy proxy;

        public LyricsReloaded(string configurationPath)
        {
            Assembly asm = Assembly.GetAssembly(GetType());
            name = asm.GetName().Name;
            pluginDirectory = Path.Combine(configurationPath, name);
            Directory.CreateDirectory(pluginDirectory);
            logger = new Logger(Path.Combine(pluginDirectory, name + ".log"));

            providerManager = new ProviderManager(this);

            providerManager.registerLoaderFactory(new StaticLoaderFactory(this));

            userAgent = "Firefox XY";
            proxy = null;
        }

        ~LyricsReloaded()
        {
            logger.close();
        }

        public Logger getLogger()
        {
            return logger;
        }

        public ProviderManager getProviderManager()
        {
            return providerManager;
        }

        public void setUserAgent(string newUserAgent)
        {
            userAgent = newUserAgent;
        }

        public string getUserAgent()
        {
            return userAgent;
        }

        public void setProxy(WebProxy newProxy)
        {
            proxy = newProxy;
        }

        public WebProxy getProxy()
        {
            return proxy;
        }

        #region "internal helpers"

        public void loadConfigurations()
        {
            loadDefaultConfiguration();

            DirectoryInfo di = new DirectoryInfo(Path.Combine(pluginDirectory, "providers"));
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
