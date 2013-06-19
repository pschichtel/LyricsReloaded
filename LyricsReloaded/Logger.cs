using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CubeIsland.LyricsReloaded
{
    public class Logger
    {
        private readonly StreamWriter writer;

        public Logger(string path)
        {
            this.writer = new StreamWriter(path, true);
        }

        private void write(string type, string message, object[] args)
        {
            this.writer.WriteLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " [" + type.ToUpper() + "] " + string.Format(message, args));
            this.writer.Flush();
        }

        public void close()
        {
            this.writer.Close();
        }

        public void debug(string message, params object[] args)
        {
            this.write("debug", message, args);
        }

        public void info(string message, params object[] args)
        {
            this.write("info", message, args);
        }

        public void warn(string message, params object[] args)
        {
            this.write("warn", message, args);
        }

        public void error(string message, params object[] args)
        {
            this.write("error", message, args);
        }
    }
}
