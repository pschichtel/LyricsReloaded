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
            Assembly asm = Assembly.GetAssembly(this.GetType());
            this.name = asm.GetName().Name;
            this.pluginDirectory = Path.Combine(configurationPath, this.name);
            Directory.CreateDirectory(this.pluginDirectory);
            this.logger = new Logger(Path.Combine(this.pluginDirectory, this.name + ".log"));

            this.providerManager = new ProviderManager(this);

            this.providerManager.registerLoaderFactory(new StaticLoaderFactory(this));

            this.userAgent = "Firefox XY";
            this.proxy = null;

            this.loadConfigurations();
        }

        ~LyricsReloaded()
        {
            this.logger.close();
        }

        public Logger getLogger()
        {
            return this.logger;
        }

        public ProviderManager getProviderManager()
        {
            return this.providerManager;
        }

        public void setUserAgent(string userAgent)
        {
            this.userAgent = userAgent;
        }

        public string getUserAgent()
        {
            return this.userAgent;
        }

        public void setProxy(WebProxy proxy)
        {
            this.proxy = proxy;
        }

        public WebProxy getProxy()
        {
            return this.proxy;
        }

        #region "internal helpers"

        public void loadConfigurations()
        {
            this.loadDefaultConfiguration();

            DirectoryInfo di = new DirectoryInfo(Path.Combine(this.pluginDirectory, "providers"));
            if (!di.Exists)
            {
                try
                {
                    di.Create();
                }
                catch (IOException e)
                {
                    this.logger.warn("Failed to create the providers folder: {0}", e.Message);
                }
            }

            foreach (FileInfo fi in di.GetFiles("*.yml", SearchOption.TopDirectoryOnly))
            {
                this.providerManager.loadProvider(fi);
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
                    this.logger.debug("Loading config from field {0}", propInfo.Name);
                    this.providerManager.loadProvider(value as String);
                }
            }
        }

        #endregion
    }
}
