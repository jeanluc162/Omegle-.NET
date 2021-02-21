using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Omegle.NET.Lib.OAS;

namespace Omegle.NET.Lib
{
    public partial class Client
    {
        private omegle_oasClient _OmegleClient = new omegle_oasClient(new System.Net.Http.HttpClient()) { BaseUrl = "https://omegle.com/"};
        private String _ClientID = null;
        private String _RandId;
        private Timer _EventsTimer;
        protected enum Errors {Antinude, General, ConnectionDied };
        public Client()
        {
            _EventsTimer = new Timer { AutoReset = false, Enabled = false, Interval = 500 };
            _EventsTimer.Elapsed += _EventsTimer_Elapsed;

            _RandId = "";
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
                _RandId += ToAdd;
            }
        }

        private async void _EventsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await HandleEvents((Events_response)await _OmegleClient.EventsAsync(new Id_params { Id = _ClientID }));
                _EventsTimer.Enabled = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Omegle.NET.Lib.Client: " + ex.Message);
                await RemoteDisconnectAsync();
            }
        }

        protected Boolean IsConnected
        {
            get
            {
                return !String.IsNullOrWhiteSpace(_ClientID);
            }
        }
        protected async Task ConnectAsync(String lang = "en", Boolean WantsSpy = false, String Question = null, String[] Topics = null, Boolean unmon = false, Boolean CanSaveQuestion = true)
        {
            if (IsConnected) throw new AlreadyConnectedException(_ClientID);
            Random rnd = new Random();
            try
            {
                Status_response status = await _OmegleClient.StatusAsync(rnd.NextDouble(), _RandId);
                _OmegleClient.BaseUrl = "https://" + status.Servers.ElementAt(rnd.Next(0, status.Servers.Count)) + ".omegle.com/";
                
                Start_params parameters = new Start_params { Randid = _RandId, Lang = lang };
                if (WantsSpy) parameters.Wantsspy = Start_paramsWantsspy._1;
                if (Question is not null && !WantsSpy) parameters.Ask = Question;
                if (Topics is not null && !WantsSpy && Question is null) parameters.Topics = Topics;
                if (unmon) parameters.Group = Start_paramsGroup.Unmon;
                if (CanSaveQuestion && Question is not null && !WantsSpy) parameters.Cansavequestion = Start_paramsCansavequestion._1;

                Start_response startresult = await _OmegleClient.StartAsync(parameters);
                _ClientID = startresult.ClientID;
                await HandleEvents(startresult.Events);
                _EventsTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Omegle.NET.Lib.Client: " + ex.Message);
                await RemoteDisconnectAsync();
            }
        }
        protected async Task DisconnectAsync()
        {
            if (!IsConnected) throw new NotConnectedException();
            try
            {
                await _OmegleClient.DisconnectAsync(new Id_params { Id = _ClientID });
                _EventsTimer.Enabled = false;
                _ClientID = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Omegle.NET.Lib.Client: " + ex.Message);
                await RemoteDisconnectAsync();
            }
        }
        protected async Task SendMessageAsync(String Message)
        {
            if (!IsConnected) throw new NotConnectedException();
            try
            {
                await _OmegleClient.SendAsync(new Send_params { Id = _ClientID, Msg = Message });
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Omegle.NET.Lib.Client: " + ex.Message);
                await RemoteDisconnectAsync();
            }
        }
        private async Task RemoteDisconnectAsync()
        {
            _EventsTimer.Enabled = false;
            _ClientID = null;
        }
        private async Task HandleEvents(Events_response Events)
        {
            foreach(Collection<String> Event in Events)
            {
                //https://gist.github.com/nucular/e19264af8d7fc8a26ece#events
                switch(Event.ElementAt(0))
                {
                    case "waiting":
                        await OnWaitingAsync();
                        break;
                    case "connected":
                        await OnConnectedAsync();
                        break;
                    case "serverMessage":
                        await OnServerMessageAsync(Event.ElementAt(1));
                        break;
                    case "commonLikes":
                        await OnCommonLikesAsync(JsonConvert.DeserializeObject<String[]>(Event.ElementAt(1)));
                        break;
                    case "error":             
                        await OnErrorDisconnectedAsync(Errors.General, Event.ElementAt(1));
                        await RemoteDisconnectAsync();
                        break;
                    case "connectionDied":
                        await OnErrorDisconnectedAsync(Errors.ConnectionDied);
                        await RemoteDisconnectAsync();
                        break;
                    case "antinudeBanned":
                        await OnErrorDisconnectedAsync(Errors.Antinude);
                        await RemoteDisconnectAsync();
                        break;
                    case "typing":
                        await OnPeerStartedTypingAsync();
                        break;
                    case "stoppedTyping":
                        await OnPeerStoppedTypingAsync();
                        break;
                    case "gotMessage":
                        await OnReceivedMessageAsync(Event.ElementAt(1));
                        break;
                    case "strangerDisconnected":
                        await OnPeerDisconnectedAsync();
                        await RemoteDisconnectAsync();
                        break;
                    case "question":
                        await OnReceivedQuestionAsync(Event.ElementAt(1));
                        break;
                    case "spyTyping":
                        if (Event.ElementAt(1).Contains("1/2"))
                        {
                            await OnPeerStartedTypingAsync(1);
                            break;
                        }
                        if (Event.ElementAt(1).Contains("2/2"))
                        {
                            await OnPeerStartedTypingAsync(2);
                            break;
                        }
                        Debug.WriteLine("Omegle.NET.Lib.Client: spyTyping: Unexpected Event Parameter: " + Event.ElementAt(1));
                        await OnPeerStartedTypingAsync();
                        break;
                    case "spyStoppedTyping":
                        if (Event.ElementAt(1).Contains("1/2"))
                        {
                            await OnPeerStoppedTypingAsync(1);
                            break;
                        }
                        if (Event.ElementAt(1).Contains("2/2"))
                        {
                            await OnPeerStoppedTypingAsync(2);
                            break;
                        }
                        Debug.WriteLine("Omegle.NET.Lib.Client: spyStoppedTyping: Unexpected Event Parameter: " + Event.ElementAt(1));
                        await OnPeerStoppedTypingAsync();
                        break;
                    case "spyMessage":
                        if (Event.ElementAt(1).Contains("1/2"))
                        {
                            await OnReceivedMessageAsync(Event.ElementAt(2), 1);
                            break;
                        }
                        if (Event.ElementAt(1).Contains("2/2"))
                        {
                            await OnReceivedMessageAsync(Event.ElementAt(2),2);
                            break;
                        }
                        Debug.WriteLine("Omegle.NET.Lib.Client: spyMessage: Unexpected Event Parameter: " + Event.ElementAt(1));
                        await OnReceivedMessageAsync(Event.ElementAt(2));
                        break;
                    case "spyDisconnected":
                        if (Event.ElementAt(1).Contains("1/2"))
                        {
                            await OnPeerDisconnectedAsync(1);
                            await RemoteDisconnectAsync();
                            break;
                        }
                        if (Event.ElementAt(1).Contains("2/2"))
                        {
                            await OnPeerDisconnectedAsync(2);
                            await RemoteDisconnectAsync();
                            break;
                        }
                        Debug.WriteLine("Omegle.NET.Lib.Client: spyStoppedTyping: Unexpected Event Parameter: " + Event.ElementAt(1));
                        await OnPeerDisconnectedAsync();
                        await RemoteDisconnectAsync();
                        break;
                    default:
                        Debug.WriteLine("Omegle.NET.Lib.Client: Unhandled Event: " + Event.ElementAt(0));
                        break;
                }
            }
        }
        protected virtual async Task OnWaitingAsync()
        {
            Debug.WriteLine("Omegle.NET.Lib.Client: Waiting for Peer(s) to connect.");
        }
        protected virtual async Task OnConnectedAsync()
        {
            Debug.WriteLine("Omegle.NET.Lib.Client: Connected to Peer(s).");
        }
        protected virtual async Task OnServerMessageAsync(String ServerMessage)
        {
            Debug.WriteLine("Omegle.NET.Lib.Client: Received Server Message: " + ServerMessage);
        }
        protected virtual async Task OnCommonLikesAsync(String[] CommonLikes)
        {
            Debug.WriteLine("Omegle.NET.Lib.Client: Sharing common topics with Peer: " + JsonConvert.SerializeObject(CommonLikes));
        }
        protected virtual async Task OnErrorDisconnectedAsync(Errors errortype, String Message = null)
        {
            Debug.WriteLine("Omegle.NET.Lib.Client: Remote Error: " + errortype.ToString());
        }
        protected virtual async Task OnPeerStartedTypingAsync(Int32? PeerID = null)
        {
            Debug.WriteLine("Omegle.NET.Lib.Client: Peer started typing.");
        }
        protected virtual async Task OnPeerStoppedTypingAsync(Int32? PeerID = null)
        {
            Debug.WriteLine("Omegle.NET.Lib.Client: Peer stopped typing.");
        }
        protected virtual async Task OnReceivedQuestionAsync(String Question)
        {
            Debug.WriteLine("Omegle.NET.Lib.Client: Received Question: " + Question);
        }
        protected virtual async Task OnReceivedMessageAsync(String Message, Int32? PeerID = null)
        {
            Debug.WriteLine("Omegle.NET.Lib.Client: Received Message from Peer: " + Message);
        }
        protected virtual async Task OnPeerDisconnectedAsync(Int32? PeerID = null)
        {
            Debug.WriteLine("Omegle.NET.Lib.Client: Peer disconnected.");
        }
    }
}
