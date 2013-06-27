using CubeIsland.LyricsReloaded.Provider;
using CubeIsland.LyricsReloaded.Provider.Loader;
using MusicBeePlugin;
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
            //this.loadProvider(Properties.Resources.songlyrics_com);
            //this.loadProvider(Properties.Resources.metrolyrics_com);
            //this.loadProvider(Properties.Resources.letras_mus_br);
            //this.loadProvider(Properties.Resources.teksty_org);
            //this.loadProvider(Properties.Resources.tekstowo_pl);
            //this.loadProvider(Properties.Resources.azlyrics_com);
            //this.loadProvider(Properties.Resources.plyrics_com);
            //this.loadProvider(Properties.Resources.urbanlyrics_com);
            //this.loadProvider(Properties.Resources.rapgenius_com);
            //this.loadProvider(Properties.Resources.oldielyrics_com);
        }


        #endregion
    }
}
