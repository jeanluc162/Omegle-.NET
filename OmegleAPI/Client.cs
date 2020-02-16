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

        private String _Server = "";
        private String _Shard = "";
        private Timer PollEvents = new Timer { Interval = 1000, AutoReset = true };
        
        public Client()
        {
            PollEvents.Elapsed += new ElapsedEventHandler(PollEvents_Elapsed);
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
                using (var PollEventsClient = new WebClient())
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
                using (var StartTypingClient = new WebClient())
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
                using (var StopTypingClient = new WebClient())
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
                using (var SendMessageClient = new WebClient())
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
        /// Connects to a random Server
        /// </summary>
        /// <returns>True if the Connection has been established, False if it failed</returns>
        public Boolean Connect()
        {
            Random randomServer = new Random();
            ReadOnlyCollection<String> CurrentlyAvailableServers = this.AvailableServers;
            return Connect(CurrentlyAvailableServers[randomServer.Next(0, CurrentlyAvailableServers.Count)]);
        }

        /// <summary>
        /// Connects to a Server
        /// </summary>
        /// <param name="Server">The Server to connect to</param>
        /// <returns>True if the Connection has been established, False if it failed</returns>
        private Boolean Connect(String Server)
        {
            if (_Shard.Length > 0) return false; //Means there already is a Connection
            String ShardString = "";
            try
            {
                using (var ShardClient = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["rcs"] = "1";
                    values["lang"] = "en";

                    var ShardResponse = ShardClient.UploadValues(URLs.Connect[0] + Server + URLs.Connect[1], values);
                    ShardString = Encoding.Default.GetString(ShardResponse).Replace("\"","");
                }
            }
            catch
            {
                return false;
            }

            if (ShardString.StartsWith("shard2"))
            {
                _Shard = ShardString;
                _Server = Server;
                PollEvents.Start();
                return true;
            }

            return false;
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
                    using (var DisconnectClient = new WebClient())
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
