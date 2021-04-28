using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omegle.NET.Lib
{
    public partial class Client
    {
        private ApiClient _OmegleApiClient;
        
        public Client()
        {
            _OmegleApiClient = new ApiClient(new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }));
            _OmegleApiClient.BaseUrl = OmegleMainServer;

            RandId = GetNewRandId();

            ConnectionState = ConnectionStates.NotConnected;
            TypingStateClient = TypingStates.Invalid;
            TypingStateStranger1 = TypingStates.Invalid;
            TypingStateStranger2 = TypingStates.Invalid;
        }
    }
}
