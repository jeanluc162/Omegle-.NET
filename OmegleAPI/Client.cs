using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Timers;

namespace OmegleAPI
{
    /// <summary>
    /// https://gist.github.com/nucular/e19264af8d7fc8a26ece
    /// </summary>
    public class Client
    {
        #region Normal Chat Events
        /// <summary>
        /// Gets invoked whenever a fatal(disconnecting) Error occures
        /// </summary>
        /// <remarks>
        /// It's best to try and reconnect after this
        /// </remarks>
        public event EventHandler Error;
        /// <summary>
        /// Gets Invoked when the Client is waiting for a Stranger to Connect
        /// </summary>
        public event EventHandler StrangerWaiting;
        /// <summary>
        /// Gets Invoked when a Stranger has Connected
        /// </summary>
        public event EventHandler StrangerConnected;
        /// <summary>
        /// Gets Invoked when a Stranger has Disconnected
        /// </summary>
        public event EventHandler StrangerDisconnected;
        /// <summary>
        /// Gets Invoked when a Stranger has started typing
        /// </summary>
        public event EventHandler StrangerTypingStarted;
        /// <summary>
        /// Gets Invoked when a Stranger has stopped typing
        /// </summary>
        public event EventHandler StrangerTypingStopped;
        /// <summary>
        /// Gets invoked when a Stranger has sent a Message
        /// </summary>
        public event StrangerSentMessageHandler StrangerSentMessage;
        public delegate void StrangerSentMessageHandler(Object sender, String Message);
        /// <summary>
        /// Gets invoked when a Stranger shares one or more of the Interests that were specified when connecting
        /// </summary>
        public event StrangerSharesInterestsHandler StrangerSharesInterest;
        public delegate void StrangerSharesInterestsHandler(Object sender, String[] Topics);
        #endregion
        #region Spy/Spyee Events
        /// <summary>
        /// Gets invoked when the Question asked by the Spy is received
        /// </summary>
        public event SpyAskedQuestionHandler SpyAskedQuestion;
        public delegate void SpyAskedQuestionHandler(Object sender, String Question);
        /// <summary>
        /// Gets invoked when one of the spyees started typing
        /// </summary>
        public event SpyeeStatusChangedHandler SpyeeTypingStarted;
        /// <summary>
        /// Gets invoked when one of the spyees stopped typing
        /// </summary>
        public event SpyeeStatusChangedHandler SpyeeTypingStopped;
        /// <summary>
        /// Gets invoked when one of the spyees disconnected
        /// </summary>
        public event SpyeeStatusChangedHandler SpyeeDisconnected;
        public delegate void SpyeeStatusChangedHandler(Object sender, Byte StrangerID);
        /// <summary>
        /// Gets invoked when one of the spyees has sent a message
        /// </summary>
        public event SpyeeSentMessageHandler SpyeeSentMessage;
        public delegate void SpyeeSentMessageHandler(Object sender, Byte StrangerID, String Message);
        #endregion

        private String _Randid = "";
        private String _Server = "";
        private String _Shard = "";
        private Timer PollEvents = new Timer { Interval = 1000, AutoReset = true };
        
        public Client()
        {   
            //Function that does the Event-Polling
            PollEvents.Elapsed += new ElapsedEventHandler(PollEvents_Elapsed);

            //Generating the randid
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
                _Randid += ToAdd;
            }
            System.Diagnostics.Debug.WriteLine("OmegleAPi.Client: Generated randid: " + _Randid);
        }
        /// <summary>
        /// Be nice to Omegle and Properly end the Session
        /// </summary>
        ~Client()
        {
            Disconnect();
        }

        /// <summary>
        /// Called regularly connected. Polls Events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PollEvents_Elapsed(object sender, ElapsedEventArgs e)
        {
            String PollEventResponse = "";
            try
            {
                using (var PollEventsClient = new OmegleWebClient())
                {
                    var values = new NameValueCollection();
                    values["id"] = _Shard;

                    PollEventResponse = Encoding.Default.GetString(PollEventsClient.UploadValues(URLs.Events[0] + _Server + URLs.Events[1], values));
                    OmegleJSON.EventResult Events = JsonConvert.DeserializeObject<OmegleJSON.EventResult>("{ \"Events\":" + PollEventResponse + "}"); //Response isn't really json, so it gets "corrected"
                    if (Events.Events == null) return;
                    foreach (List<Object> Event in Events.Events)
                    {
                        if (Event.Count == 0) continue;
                        if (Event[0].GetType() != typeof(String)) continue;
                        System.Diagnostics.Debug.WriteLine("OmegleAPi.Client: Received Event: " + Event[0]);
                        switch ((String)Event[0])
                        {
                            case "waiting": if (StrangerWaiting != null) StrangerWaiting(this, new EventArgs()); break;
                            case "connected": if (StrangerConnected != null) StrangerConnected(this, new EventArgs()); break;
                            case "strangerDisconnected": if (StrangerDisconnected != null) StrangerDisconnected(this, new EventArgs()); break;
                            case "gotMessage": if (StrangerSentMessage != null) StrangerSentMessage(this, (String)Event[1]); break;
                            case "typing": if (StrangerTypingStarted != null) StrangerTypingStarted(this, new EventArgs()); break;
                            case "stoppedTyping": if (StrangerTypingStopped != null) StrangerTypingStopped(this, new EventArgs()); break;
                            case "error": if (Error != null) Error(this, new EventArgs()); break;
                            case "connectionDied": if (Error != null) Error(this, new EventArgs()); break;
                            case "antinudeBanned": if (Error != null) Error(this, new EventArgs()); break;
                            case "question": if (SpyAskedQuestion != null) SpyAskedQuestion(this, (String)Event[1]); break;
                            case "spyTyping":   
                                if (SpyeeTypingStarted != null)
                                {
                                    if ((String)Event[1] == "Stranger <1/2>") SpyeeTypingStarted(this, 1);
                                    else if ((String)Event[1] == "Stranger <2/2>") SpyeeTypingStarted(this, 2);
                                }
                                break;
                            case "spyStoppedTyping":
                                if (SpyeeTypingStopped != null)
                                {
                                    if ((String)Event[1] == "Stranger <1/2>") SpyeeTypingStopped(this, 1);
                                    else if ((String)Event[1] == "Stranger <2/2>") SpyeeTypingStopped(this, 2);
                                }
                                break;
                            case "spyMessage":
                                if (SpyeeSentMessage!= null)
                                {
                                    if ((String)Event[1] == "Stranger <1/2>") SpyeeSentMessage(this, 1,(String)Event[2]);
                                    else if ((String)Event[1] == "Stranger <2/2>") SpyeeSentMessage(this, 2, (String)Event[2]);
                                }
                                break;
                            case "spyDisconnected":
                                if (SpyeeDisconnected != null)
                                {
                                    if ((String)Event[1] == "Stranger <1/2>") SpyeeDisconnected(this, 1);
                                    else if ((String)Event[1] == "Stranger <2/2>") SpyeeDisconnected(this, 2);
                                }
                                break;
                            case "commonLikes":
                                System.Diagnostics.Debug.WriteLine(Event[1].GetType().ToString());
                                break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(PollEventResponse);
                if (Error != null) Error(this, new EventArgs());
            }
        }
   
        /// <summary>
        /// Starts emulated typing
        /// </summary>
        /// <returns></returns>
        public Boolean StartTyping()
        {
            try
            {
                using (var StartTypingClient = new OmegleWebClient())
                {
                    var values = new NameValueCollection();
                    values["id"] = _Shard;

                    if (Encoding.Default.GetString(StartTypingClient.UploadValues(URLs.TypingStart[0] + _Server + URLs.TypingStart[1], values)) == "win") return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                if (Error != null) Error(this, new EventArgs());
            }
            return false;
        }

        /// <summary>
        /// Stops emulated typing
        /// </summary>
        /// <returns></returns>
        public Boolean StopTyping()
        {
            try
            {
                using (var StopTypingClient = new OmegleWebClient())
                {
                    var values = new NameValueCollection();
                    values["id"] = _Shard;

                    if (Encoding.Default.GetString(StopTypingClient.UploadValues(URLs.TypingStop[0] + _Server + URLs.TypingStop[1], values)) == "win") return true;
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                if (Error != null) Error(this, new EventArgs());
            }
            return false;
        }

        public Boolean SendMessage(String Message)
        {
            try
            {
                using (var SendMessageClient = new OmegleWebClient())
                {
                    var values = new NameValueCollection();
                    values["id"] = _Shard;
                    values["msg"] = Message;

                    if (Encoding.Default.GetString(SendMessageClient.UploadValues(URLs.Send[0] + _Server + URLs.Send[1], values)) == "win") return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                if (Error != null) Error(this, new EventArgs());
            }
            return false;
        }

        /// <summary>
        /// A List of all Omegle-Servers that are currently available
        /// </summary>
        private ReadOnlyCollection<String> AvailableServers
        {
            get
            {
                try
                {
                    OmegleJSON.Status Status;
                    using (var StatusClient = new WebClient())
                    {
                        Status = JsonConvert.DeserializeObject<OmegleJSON.Status>(StatusClient.DownloadString(URLs.Status));
                    }

                    for (int i = 0; i < Status.servers.Count; i++)
                    {
                        Status.servers[i] += "." + URLs.Omegle;
                    }

                    return Status.servers.AsReadOnly();
                }
                catch
                {
                    return new ReadOnlyCollection<String>(new List<String>());
                }
            }
        }

        /// <summary>
        /// Attempts to connect to a chat where two Strangers discuss a Question
        /// </summary>
        /// <param name="Question">The Question to be discussed</param>
        /// <returns>True if the attempt was successfull, False if it wasn't</returns>
        public Boolean ConnectChatSpy(String Question)
        {
            return Connect(null, false, Question, null, false);
        }

        /// <summary>
        /// Attempts to Connect to a Stranger to Discuss a Question posed by a third (spying) Party with.
        /// </summary>
        /// <returns>True if the attempt was successfull, False if it wasn't</returns>
        public Boolean ConnectChatSpyee()
        {
            return Connect(null, true, null, null, false);
        }

        /// <summary>
        /// Attempts to Connect to a Stranger with overlapping Interest
        /// </summary>
        /// <param name="Topics">A List of Topics to be discussed</param>
        /// <returns>True if the attempt was successfull, False if it wasn't</returns>
        public Boolean ConnectChatInterest(String[] Topics)
        {
            return Connect(null, false, null, Topics, false);
        }
        
        /// <summary>
        /// Attempts to Connect to a Server in Normal Chat Mode
        /// </summary>
        /// <param name="Language">The desired Chat language (eg. "en" or "de"). If null, english is used. Only Seems to Work properly in Normal Chat mode</param>
        /// <param name="Unmoderated">If true Connects to the unmoderated section</param>
        /// <returns>True if the attempt was successfull, False if it wasn't</returns>
        public Boolean ConnectChat(String Language, Boolean Unmoderated)
        {
            return Connect(Language, false, null, null, Unmoderated) ;
        }

        /// <summary>
        /// Attempts to connect to a Server
        /// </summary>
        /// <param name="Language">The desired Chat language (eg. "en" or "de"). If null, english is used</param>
        /// <param name="Spyee">Enables Spyee-Mode (a third party watches you discuss a question with a stranger)</param>
        /// <param name="Question">Enables Spy-Mode (The Question gets passed to two Strangers). Parameter is ignored if <c>Spyee</c> is set to true</param>
        /// <param name="topics">Attempts to connect to a Stranger with overlapping interests. Parameter is ignored if <c>Spyee</c> is set to true or if <c>Question</c> is not empty</param>
        /// <remarks>Is not to be called directly (Does not check the combination of parameters for validity). Usage of the <c>ConnectChat*</c> Functions is advised</remarks>
        /// <returns>True if the attempt was successfull, False if it wasn't</returns>
        private Boolean Connect(String Language, Boolean Spyee, String Question, String[] topics, Boolean Unmoderated)
        {            
            if (_Shard.Length > 0) return false; //Means there already is a Connection
            _Server = RandomServer();
            System.Diagnostics.Debug.WriteLine("OmegleAPI.Client: Decided on Server: " + _Server);

            NameValueCollection Parameters = new NameValueCollection();
            
            //Has No deeper Meaning
            //Parameters["rcs"] = "1";
            Parameters["spid"] = "";
            Parameters["caps"] = "rechapta2";
            Parameters["firstevents"] = "0";
            Parameters["randid"] = _Randid;

            //Unmoderated
            if (Unmoderated) Parameters["group"] = "unmon";

            //Language to Use
            if (Language != null) Parameters["lang"] = Language;
            else Parameters["lang"] = "en";
            
            //Ask Question as Spy
            if (Question != null) Parameters["ask"] = Question;

            //Discuss a third party Question
            if (Spyee) Parameters["wantsspy"] = "1";

            //Search for common Interest
            if (topics != null)
            {
                if (topics.Length > 0)
                {
                    Parameters["topics"] = "[";
                    Parameters["topics"] += "\"" + topics[0] + "\"";
                    for (int i = 1; i < topics.Length; i++) Parameters["topics"] += ", \"" + topics[i] + "\"";
                    Parameters["topics"] += "]";
                }
            }

            //Attempt to Connect to the chosen Server
            try
            {
                System.Diagnostics.Debug.WriteLine("OmegleApi.Client: Requesting Shard with the following Parameters:");
                foreach (String Key in Parameters.Keys)
                {
                    System.Diagnostics.Debug.WriteLine("OmegleApi.Client: [" + Key + " : " + Parameters[Key] + "]");    
                }
                using (var ShardClient = new OmegleWebClient())
                {
                    var ShardResponse = ShardClient.UploadValues(URLs.Connect[0] + _Server + URLs.Connect[1], Parameters);
                    _Shard = Encoding.Default.GetString(ShardResponse).Replace("\"","");
                    System.Diagnostics.Debug.WriteLine("OmegleAPI.Client: Received Shard: " + _Shard);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("OmegleAPI.Client: Error while requesting Shard: " + ex.Message);
                _Server = "";
                _Shard = "";
                return false;
            }

            if (_Shard.Length == 0)
            {
                _Shard = "";
                _Server = "";
                return false;
            }
            else
            {
                PollEvents.Start();
                return true;
            }          
        }

        /// <summary>
        /// Determines a Random Server
        /// </summary>
        /// <returns>The random Server</returns>
        private String RandomServer()
        {
            Random randomServer = new Random();
            ReadOnlyCollection<String> CurrentlyAvailableServers = this.AvailableServers;
            return CurrentlyAvailableServers[randomServer.Next(0, CurrentlyAvailableServers.Count)];
        }

        /// <summary>
        /// Closes the Connection to the Server (if it is open)
        /// </summary>
        public void Disconnect()
        {
            if (_Shard.Length > 0)
            {
                try
                {
                    using (var DisconnectClient = new OmegleWebClient())
                    {
                        var values = new NameValueCollection();
                        values["id"] = _Shard;

                        DisconnectClient.UploadValues(URLs.Disconnect[0] + _Server + URLs.Disconnect[1], values);
                    }
                }
                catch
                {

                }
                _Shard = "";
                _Server = "";
                PollEvents.Stop();
            }
        }
    }
}
