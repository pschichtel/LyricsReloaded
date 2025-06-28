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
    public class SearchLoader : LyricsLoader
    {
        private readonly LyricsReloaded lyricsReloaded;
        private readonly string urlLyrics;
        private readonly Pattern patternLyrics;
		private readonly string urlIdentity;
        private readonly Pattern patternIdentity;
        private readonly WebClient client;

        public SearchLoader(LyricsReloaded lyricsReloaded, WebClient client, string urlLyrics, Pattern patternLyrics, string urlIdentity, Pattern patternIdentity)
        {
            this.lyricsReloaded = lyricsReloaded;
            this.urlLyrics = urlLyrics;
            this.patternLyrics = patternLyrics;
			this.urlIdentity = urlIdentity;
            this.patternIdentity = patternIdentity;
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
			string urlSearch = constructData(urlIdentity, variables);
            Pattern patternSearch = patternIdentity;
			
			if (Regex.IsMatch(urlSearch, "<identity>") == true)
			{
				urlSearch = getGoogleUrl(provider, variables, urlSearch);
				
				if (urlSearch == null)
				{
					return null;
				}
			}
			
			if (Regex.IsMatch(patternIdentity.ToString(), "{title}|{artist}|{album}") == true)
			{
				string regexIdentity = constructData(patternIdentity.ToString(),variables);
				patternSearch = new Pattern(regexIdentity, "is");
			}
		
			lyricsReloaded.getLogger().debug("The constructed identity URL: {0}", urlSearch);
			
            try
            {
                WebResponse responseIdentity = client.get(urlSearch, provider.getHeaders());
                String identity = patternSearch.apply(responseIdentity.getContent(), "identity");
                if (identity == null)
                {
                    lyricsReloaded.getLogger().warn("The identity pattern {0} didn't match!", patternSearch);
                    return null;
                }
				
				string urlLyrics = constructData(this.urlLyrics, variables) + identity;
				lyricsReloaded.getLogger().debug("The constructed lyrics URL: {0}", urlLyrics);
				
				try
				{
					WebResponse response = client.get(urlLyrics, provider.getHeaders());
					String lyrics = patternLyrics.apply(response.getContent(), "lyrics");
					if (lyrics == null)
					{
						lyricsReloaded.getLogger().warn("The lyrics pattern {0} didn't match!", patternLyrics);
						return null;
					}

					return new Lyrics(lyrics, response.getEncoding());
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
            catch (WebException e)
            {
                if (isStatus(e, HttpStatusCode.NotFound))
                {
                    return null;
                }
                throw;
            }	
        }
		
		private string getGoogleUrl(Provider provider, Dictionary<string, string> variables, string regexGoogle)
		{
			string urlGoogle = constructData("https://www.google.com/search?q=" + provider.getName() + "+{artist}+{title}+{album}", variables);
			lyricsReloaded.getLogger().debug("The constructed google identity URL: {0}", urlGoogle);
			Pattern patternGoogle = new Pattern(regexGoogle, "is");
			
			try
			{
				WebResponse responseIdentityGoogle = client.get(urlGoogle, provider.getHeaders());
				String identityGoogle = patternGoogle.apply(responseIdentityGoogle.getContent(), "identity");
				if (identityGoogle == null)
				{
					lyricsReloaded.getLogger().warn("The google identity pattern {0} didn't match!", patternGoogle);
					return null;
				}
				
				return identityGoogle;
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

    public class SearchLoaderFactory : LyricsLoaderFactory
    {
        private static class Node
        {
			public static readonly YamlScalarNode IDENTITYURL = new YamlScalarNode("identity url");
            public static readonly YamlScalarNode IDENTITYPATTERN = new YamlScalarNode("identity pattern");
            public static readonly YamlScalarNode LYRICSURL = new YamlScalarNode("lyrics url");
            public static readonly YamlScalarNode LYRICSPATTERN = new YamlScalarNode("lyrics pattern");
        }

        private readonly LyricsReloaded lyricsReloaded;
        private readonly WebClient webClient;

        public SearchLoaderFactory(LyricsReloaded lyricsReloaded)
        {
            this.lyricsReloaded = lyricsReloaded;
            webClient = new WebClient(lyricsReloaded, 5000);
        }

        public string getName()
        {
            return "search";
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

            if (!configNodes.ContainsKey(Node.LYRICSPATTERN))
            {
                throw new InvalidConfigurationException("No lyrics pattern specified!");
            }
			
			string identityurl;
            node = (configNodes.ContainsKey(Node.IDENTITYURL) ? configNodes[Node.IDENTITYURL] : null);
            if (node is YamlScalarNode)
            {
                identityurl = ((YamlScalarNode)node).Value;
            }
            else
            {
                throw new InvalidConfigurationException("No identity URL specified!");
            }

            if (!configNodes.ContainsKey(Node.IDENTITYPATTERN))
            {
                throw new InvalidConfigurationException("No identity pattern specified!");
            }
			
            return new SearchLoader(lyricsReloaded, webClient, lyricsurl, Pattern.fromYamlNode(configNodes[Node.LYRICSPATTERN]), identityurl, Pattern.fromYamlNode(configNodes[Node.IDENTITYPATTERN]));
        }
    }
}