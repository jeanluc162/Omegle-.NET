using System;
using System.Threading.Tasks;

namespace OmegleLibrary
{
    public class Client
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

        public Client()
        {
            RandomID = GetRandomID();
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
            return;
        }
        protected virtual async Task OnQuestionReceivedAsync(String Question)
        {
            return;
        }
        protected virtual async Task OnConnectedAsync()
        {
            return;
        }
        protected virtual async Task OnStrangerDisconnectedAsync(Int32 Stranger)
        {
            return;
        }
        protected virtual async Task OnErrorAsync()
        {
            return;
        }
        protected async Task<Boolean> SendMessageAsync(String Message)
        {
            throw new NotImplementedException();
        }
        public async Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }
        public async Task StartAsync()
        {
            throw new NotImplementedException();
        }
    }
}
