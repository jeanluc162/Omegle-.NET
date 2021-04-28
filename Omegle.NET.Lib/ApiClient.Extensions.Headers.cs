using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omegle.NET.Lib
{
    internal partial class ApiClient
    {
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Origin", "https://www.omegle.com");
            request.Headers.Add("Referer", "https://www.omegle.com/");
            request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.182 Safari/537.36 Edg/88.0.705.74");
        }
    }
}
