/*
    Copyright 2013 Phillip Schichtel
	Created by Mayibongwe (2022) using Phillip's static loader as a basis.
	
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
using System.Text.RegularExpressions;
using System.Net;
using YamlDotNet.RepresentationModel;

namespace CubeIsland.LyricsReloaded.Provider.Loader
{
    public class apiLoader : LyricsLoader
    {
        private readonly LyricsReloaded lyricsReloaded;
        private readonly string urlLyrics;
		private readonly string urlToken;
        private readonly WebClient client;

        public apiLoader(LyricsReloaded lyricsReloaded, WebClient client, string urlLyrics, string urlToken)
        {
            this.lyricsReloaded = lyricsReloaded;
            this.urlLyrics = urlLyrics;
			this.urlToken = urlToken;
            this.client = client;
        }

        private string constructData(string data, Dictionary<string, string> variables)
        {
            foreach (KeyValuePair<string, string> entry in variables)
            {
                data = data.Replace("{" + entry.Key + "}", entry.Value);
            }

            return data;
        }

        public Lyrics getLyrics(Provider provider, Dictionary<string, string> variables)
        {
			string url = constructData(urlLyrics, variables);
			lyricsReloaded.getLogger().debug("The constructed URL: {0}",url);
			
			try
			{
				WebResponse response = client.get(url, provider.getHeaders());
				return new Lyrics(response.getContent(), response.getEncoding());
			}
			catch (WebException e)
			{
				if (isStatus(e, HttpStatusCode.NotFound))
				{
					return null;
				}
				throw;
			}
        }
		
		private static bool isStatus(WebException e, HttpStatusCode code)
        {
            System.Net.WebResponse r = e.Response;
            if (r == null)
            {
                if (e.InnerException != null && e.InnerException is WebException)
                {
                    return isStatus(e.InnerException as WebException, code);
                }
            }
            else if (r is HttpWebResponse && ((HttpWebResponse)r).StatusCode == code)
            {
                return true;
            }
            return false;
        }
    }

    public class apiLoaderFactory : LyricsLoaderFactory
    {
        private static class Node
        {
			public static readonly YamlScalarNode LYRICSURL = new YamlScalarNode("url");
            public static readonly YamlScalarNode TOKEN = new YamlScalarNode("token");
        }

        private readonly LyricsReloaded lyricsReloaded;
        private readonly WebClient webClient;

        public apiLoaderFactory(LyricsReloaded lyricsReloaded)
        {
            this.lyricsReloaded = lyricsReloaded;
            webClient = new WebClient(lyricsReloaded, 5000);
        }

        public string getName()
        {
            return "API";
        }

        public LyricsLoader newLoader(YamlMappingNode configuration)
        {
            YamlNode node;
            IDictionary<YamlNode, YamlNode> configNodes = configuration.Children;

            string lyricsurl;
            node = (configNodes.ContainsKey(Node.LYRICSURL) ? configNodes[Node.LYRICSURL] : null);
            if (node is YamlScalarNode)
            {
                lyricsurl = ((YamlScalarNode)node).Value;
            }
            else
            {
                throw new InvalidConfigurationException("No lyrics URL specified!");
            }
			
			string token;
            node = (configNodes.ContainsKey(Node.TOKEN) ? configNodes[Node.TOKEN] : null);
            if (node is YamlScalarNode)
            {
                token = ((YamlScalarNode)node).Value;
            }
            else
            {
                throw new InvalidConfigurationException("No token specified!");
            }
			
			return new apiLoader(lyricsReloaded, webClient, lyricsurl, token);
        }
    }
}