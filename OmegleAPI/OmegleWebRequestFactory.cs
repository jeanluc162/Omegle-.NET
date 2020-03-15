using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace OmegleAPI
{
    internal static class OmegleWebRequestFactory
    {
        public static HttpWebRequest CreateRequest(String URL, Dictionary<String, String> Parameters)
        {            
            if (Parameters != null)
            {
                for (int i = 0; i < Parameters.Keys.Count; i++)
                {
                    if (i == 0) URL += "?";
                    else URL += "&";

                    URL += Parameters.Keys.ElementAt(i) + "=";
                    URL += Uri.EscapeUriString(Parameters[Parameters.Keys.ElementAt(i)]);
                }
            }
            
            HttpWebRequest OmegleRequest = (HttpWebRequest)WebRequest.Create(URL);
            OmegleRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            OmegleRequest.Method = "POST";
            OmegleRequest.ContentLength = 0;
            OmegleRequest.ContentType = "application/x-www-form-urlencoded";
            OmegleRequest.Accept = "application/json";
            OmegleRequest.Referer = "http://" + URLs.Omegle;
            OmegleRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.94 Safari/537.36";

            OmegleRequest.Headers["Accept-Language"] = "en-US;q=0.6,en;q=0.4";
            OmegleRequest.Headers["DNT"] = "1";
            OmegleRequest.Headers["Origin"] = "http://" + URLs.Omegle;
            OmegleRequest.KeepAlive = true;

            return OmegleRequest;
        }
    }
}
