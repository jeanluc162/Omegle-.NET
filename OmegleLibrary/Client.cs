using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

namespace OmegleLibrary
{
    //https://gist.github.com/nucular/e19264af8d7fc8a26ece
    public class Client:IDisposable
    {
        private static class URLs
        {
            public const String OmegleMain = "omegle.com";
            public const String Status = "/status";
            public const String Start = "/start";
            public const String Events = "/events";
            public const String TypingStart = "/typing";
            public const String TypingStop = "/stoppedtyping";
            public const String Disconnect = "/disconnect";
            public const String Send = "/send";
            public const String StopCommonLikes = "/stoplookingforcommonlikes";
        }

        private readonly String RandomID;
        private String CurrentHost;
        private HttpClient OmegleClient;
        private Boolean disposed;
        private String StartParameters = "";
        private String ClientID = "";
        public Client(IWebProxy Proxy = null)
        {
            disposed = false;

            //Generate the Random ID
            RandomID = GetRandomID();

            //Set the current Host to the Main Omegle Host
            CurrentHost = URLs.OmegleMain;

            //Prepare the HTTPClient
            HttpClientHandler OmegleHandler = new HttpClientHandler();
            OmegleHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
            if(Proxy != null) OmegleHandler.Proxy = Proxy;
            
            OmegleClient = new HttpClient(OmegleHandler);
            OmegleClient.DefaultRequestHeaders.Clear();
            OmegleClient.DefaultRequestHeaders.Add("Accept", "application/json");
            OmegleClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
            OmegleClient.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.6,en;q=0.4");
            OmegleClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            OmegleClient.DefaultRequestHeaders.Add("DNT", "1");
            OmegleClient.DefaultRequestHeaders.Add("Host", CurrentHost);
            OmegleClient.DefaultRequestHeaders.Add("Origin", "https://www.omegle.com/");
            OmegleClient.DefaultRequestHeaders.Add("Referer", "https://www.omegle.com/");
            OmegleClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2062.94 Safari/537.36");
        }
        private String GetRandomID()
        {
            String NewRandomID = "";
            Random RandidGenerator = new Random();
            for (int i = 0; i < 8; i++)
            {
                char ToAdd = ' ';
                int num = 34;
                while (num == 34)
                {
                    num = RandidGenerator.Next(0, 34);
                    if (num >= 26) ToAdd = (num - 24).ToString()[0];
                    else
                    {
                        ToAdd = (char)('A' + num);
                        if (ToAdd == 'I' || ToAdd == 'O') num = 34;
                    }
                }
                NewRandomID += ToAdd;
            }
            return NewRandomID;
        }

        protected virtual async Task OnMessageReceivedAsync(String Message, Int32 Stranger)
        {
            System.Diagnostics.Debug.WriteLine("OmegleLibrary: Client: Message Received: " + Stranger.ToString() + ": " + Message);
            return;
        }
        protected virtual async Task OnQuestionReceivedAsync(String Question)
        {
            System.Diagnostics.Debug.WriteLine("OmegleLibrary: Client: Question Received: " + Question);
            return;
        }
        protected virtual async Task OnConnectedAsync()
        {
            System.Diagnostics.Debug.WriteLine("OmegleLibrary: Client: Connected");
            return;
        }
        protected virtual async Task OnStrangerDisconnectedAsync(Int32 Stranger)
        {
            System.Diagnostics.Debug.WriteLine("OmegleLibrary: Client: Stranger Disconnected: " + Stranger.ToString());
            return;
        }
        protected virtual async Task OnErrorAsync()
        {
            System.Diagnostics.Debug.WriteLine("OmegleLibrary: Client: Error");
            return;
        }
        protected virtual async Task OnSharedInterestsAsync(String[] SharedInterests)
        {
            String SharedInterestsString = "";
            foreach(String SharedInterest in SharedInterests)
            {
                if(SharedInterestsString.Length > 0) SharedInterestsString += "|";
                SharedInterestsString += SharedInterest;
            }
            System.Diagnostics.Debug.WriteLine("OmegleLibrary: Client: Shared Interests: " + SharedInterestsString);
            return;
        }
        protected async Task<Boolean> SendMessageAsync(String Message)
        {
            throw new NotImplementedException();
        }
        protected async Task DisconnectAsync()
        {
            if(ClientID.Length == 0) throw new Exception("Client is not connected!");
            FormUrlEncodedContent DisconnectContent = new FormUrlEncodedContent(new[]{
                new KeyValuePair<String,String>("id", ClientID)
            });

            await OmegleClient.PostAsync("https://" + CurrentHost + URLs.Disconnect, DisconnectContent);
            ClientID = "";
        }
        protected async Task StartAsync()
        {
            StatusResponse status = await OmegleClient.GetFromJsonAsync<StatusResponse>("https://" + URLs.OmegleMain + URLs.Status + "?" + WebUtility.UrlEncode("randid=" + RandomID));   
            Random rnd = new Random();
            CurrentHost = status.servers[rnd.Next(0,status.servers.Length)] + "." + URLs.OmegleMain;
            HttpResponseMessage StartResponseMessage = await OmegleClient.PostAsync("https://" + CurrentHost + URLs.Start + "?" +  StartParameters + "&firstevents=1&rcs=1", null);
            StartResponse start = await StartResponseMessage.Content.ReadFromJsonAsync<StartResponse>(); 
            ClientID = start.clientID;        
        }
        protected async Task StartNormalAsync(String Language = "en", String[] Topics = null, Boolean Unmonitored=false)
        {
            StartParameters = "&randid=" + RandomID + "&lang=" + Language;
            if(Topics != null)
            {
                StartParameters += "&topics=";
                String TopicsString = "[";
                foreach (String Topic in Topics)
                {
                    if(TopicsString.Length > 1) TopicsString += ",";
                    TopicsString += "\"" + Topic + "\"";
                }
                TopicsString += "]";
                StartParameters += WebUtility.UrlEncode(TopicsString);
            }
            if(Unmonitored) StartParameters += "&group=unmon";

            await StartAsync();
        }

        //https://stackoverflow.com/a/31016954
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    OmegleClient.Dispose();
                }
                disposed = true;
            }
        }
    }
}
