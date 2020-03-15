using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace OmegleAPI
{
    internal class OmegleWebClient:WebClient
    {
        public OmegleWebClient(): base()
        {
            this.Headers["Accept"] = "application/json";
            this.Headers["Accept-Encoding"] = "gzip, deflate, br";
            this.Headers["Accept-Language"] = "en-US;q=0.6,en;q=0.4";
            this.Headers["DNT"] = "1";
            this.Headers["Origin"] = "http://" + URLs.Omegle;
            this.Headers["Referer"] = "http://" + URLs.Omegle + "/";
            this.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.94 Safari/537.36";
        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
            return request;
        }
    }
}
