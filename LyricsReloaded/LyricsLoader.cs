using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace CubeIsland.LyricsReloaded
{
    public class LyricsLoader
    {
        private static readonly Regex ENCODING_REGEX = new Regex("<meta\\s+http-equiv=[\"']?content-type[\"']?\\s+content=.*?;\\s*charset\\s*=\\s*([a-z0-9-]+)[^>]*>|<\\?xml.+?encoding=\"([^\"]).*?\\?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private WebProxy proxy;
        private readonly int timeout;

        public LyricsLoader(int timeout)
        {
            this.timeout = timeout;
        }

        public int getTimeout()
        {
            return this.timeout;
        }

        public LyricsResponse loadContent(string url, String userAgent)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

            request.Method = "GET";
            if (userAgent != null)
            {
                request.UserAgent = userAgent;
            }
            //request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            request.Headers.Add("Accept-Encoding", "gzip");
            request.ContentLength = 0;
            if (this.proxy != null)
            {
                request.Proxy = this.proxy;
            }

            IAsyncResult result;
            try
            {
                result = request.BeginGetResponse(null, null);
                if (!result.AsyncWaitHandle.WaitOne(this.timeout, false))
                {
                    throw new WebException("The operation has timed out.", WebExceptionStatus.Timeout);
                }
            }
            catch
            {
                try
                {
                    request.Abort();
                }
                catch
                {}
                throw;
            }

            String contentString = null;
            Encoding encoding = Encoding.ASCII;
            using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result))
            {
                if (response.CharacterSet != null)
                {
                    encoding = Encoding.GetEncoding(response.CharacterSet);
                }

                Stream responsesStream = response.GetResponseStream();
                if (String.Compare(response.ContentEncoding, "gzip", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    responsesStream = new GZipStream(responsesStream, CompressionMode.Decompress);
                }
                MemoryStream content = new MemoryStream();
                const int BUFFER_SIZE = 4096;
                byte[] buffer = new byte[BUFFER_SIZE];
                int bytesRead = 0;
                do
                {
                    bytesRead = responsesStream.Read(buffer, 0, BUFFER_SIZE);
                    if (bytesRead <= 0)
                    {
                        break;
                    }
                    content.Write(buffer, 0, bytesRead);
                }
                while (true);
                responsesStream.Close();

                contentString = encoding.GetString(content.GetBuffer(), 0, Convert.ToInt32(content.Length));
                Match match = ENCODING_REGEX.Match(contentString);
                if (match.Success)
                {
                    try
                    {
                        Encoding tmp = Encoding.GetEncoding(match.Groups[1].ToString());
                        if (tmp != null && encoding != tmp)
                        {
                            encoding = tmp;
                            contentString = encoding.GetString(content.GetBuffer(), 0, Convert.ToInt32(content.Length));
                        }
                    }
                    catch (ArgumentException)
                    {}
                }
                content.Close();
            }
            
            return new LyricsResponse(contentString, encoding);
        }

        public void setProxy(WebProxy proxy)
        {
            this.proxy = proxy;
        }
    }

    public class LyricsResponse
    {
        private readonly string content;
        private readonly Encoding encoding;

        public LyricsResponse(string content, Encoding encoding)
        {
            this.content = content;
            this.encoding = encoding;
        }

        public string getContent()
        {
            return this.content;
        }

        public Encoding getEncoding()
        {
            return this.encoding;
        }
    }
}
