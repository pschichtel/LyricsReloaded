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
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace CubeIsland.LyricsReloaded
{
    public class Logger
    {
        private readonly BlockingCollection<string> messageQueue;
        private readonly Thread writerThread;
        private readonly FileInfo fileInfo;
        private StreamWriter writer;

        public Logger(string path)
        {
            fileInfo = new FileInfo(path);
            writer = new StreamWriter(fileInfo.FullName, true, Encoding.UTF8) {
                AutoFlush = false
            };

            messageQueue = new BlockingCollection<string>();
            writerThread = new Thread(write) {
                IsBackground = false,
                Name = "LyricsReloaded - Logging"
            };
            writerThread.Start();
        }

        public FileInfo getFileInfo()
        {
            return fileInfo;
        }

        private void write()
        {
            while (true)
            {
                writer.WriteLine(messageQueue.Take());
                writer.Flush();
            }
        }

        private void queueMessage(string type, string message, object[] args)
        {
            messageQueue.Add(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " [" + type.ToUpper() + "] " + string.Format(message, args));
        }

        public void close()
        {
            if (!writerThread.Join(5000))
            {
                writerThread.Interrupt();
            }
            try
            {
                writer.Close();
            }
            catch (ObjectDisposedException)
            {}
        }

        public void debug(string message, params object[] args)
        {
            queueMessage("debug", message, args);
        }

        public void info(string message, params object[] args)
        {
            queueMessage("info", message, args);
        }

        public void warn(string message, params object[] args)
        {
            queueMessage("warn", message, args);
        }

        public void error(string message, params object[] args)
        {
            queueMessage("error", message, args);
        }
    }
}
