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
using System.Text.RegularExpressions;

namespace CubeIsland.LyricsReloaded
{
    public class Logger
    {
        private readonly FileInfo fileInfo;
        private StreamWriter writer;

        public Logger(string path)
        {
            fileInfo = new FileInfo(path);
            writer = null;
        }

        public FileInfo getFileInfo()
        {
            return fileInfo;
        }

        private void write(string type, string message, object[] args)
        {
            if (writer == null)
            {
                writer = new StreamWriter(fileInfo.FullName, true, Encoding.UTF8);
                writer.AutoFlush = false;
            }
			
			// <additions> by Mayibongwe
			string newLine = "";
			if (type == "debug" && message.Contains("Lyrics request") == true)
			{
				newLine = Environment.NewLine;
			}
			// </additions>
			
			writer.WriteLine(DateTime.Now.ToString(newLine + "dd/MM/yyyy hh:mm:ss") + " [" + type.ToUpper() + "] " + string.Format(message, args));
            writer.Flush();
        }

        public void close()
        {
            if (writer != null)
            {
                try
                {
                    writer.Close();
                }
                catch (ObjectDisposedException)
                {}
            }
        }
		
		// <additions> by Mayibongwe
		public static void clearLog(string path)
		{	
			if(File.Exists(path))
			{
				string content = "";
				int numMatches;
				
				using (StreamReader reader = File.OpenText(path))
				{
					content = reader.ReadToEnd();
				}
				
				MatchCollection matches = Regex.Matches(content, @"version.*?started!");
				numMatches = matches.Count;
				
				if(numMatches >= 10)
				{
					File.WriteAllText(path, "");
				}
			}
		}
		
		public void success(string message, params object[] args)
        {
            write("success", message, args);
        }
		
		public void fail(string message, params object[] args)
        {
            write("fail", message, args);
        }
		// </additions>
		
		public void debug(string message, params object[] args)
        {
            write("debug", message, args);
        }

        public void info(string message, params object[] args)
        {
            write("info", message, args);
        }

        public void warn(string message, params object[] args)
        {
            write("warn", message, args);
        }

        public void error(string message, params object[] args)
        {
            write("error", message, args);
        }
    }
}
