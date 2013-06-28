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
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CubeIsland.LyricsReloaded
{
    public class Logger
    {
        private readonly FileInfo fileInfo;
        private StreamWriter writer;

        public Logger(string path)
        {
            this.fileInfo = new FileInfo(path);
            this.writer = null;
        }

        public FileInfo getFileInfo()
        {
            return this.fileInfo;
        }

        private void write(string type, string message, object[] args)
        {
            if (this.writer == null)
            {
                this.writer = new StreamWriter(this.fileInfo.FullName, true, Encoding.UTF8);
                this.writer.AutoFlush = false;
            }
            this.writer.WriteLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " [" + type.ToUpper() + "] " + string.Format(message, args));
            this.writer.Flush();
        }

        public void close()
        {
            if (this.writer != null)
            {
                try
                {
                    this.writer.Close();
                }
                catch (ObjectDisposedException)
                {}
            }
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
