using System;
using OmegleAPI;

namespace OmegleCommandLine
{
    internal class Program
    {
        enum MessageIssuer { Self, Stranger }
        static Client MyClient;
        const String WindowTitle = "Omegle - ";
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BufferWidth = 130;
            Console.WindowWidth = 130;

            MyClient = new Client();
            MyClient.Error += new EventHandler(MyClient_Error);
            MyClient.StrangerConnected += new EventHandler(MyClient_StrangerConnected);
            MyClient.StrangerDisconnected += new EventHandler(MyClient_StrangerDisconnected);
            MyClient.StrangerSentMessage += new Client.StrangerSentMessageHandler(MyClient_StrangerSentMessage);
            MyClient.StrangerTypingStarted += new EventHandler(MyClient_StrangerTypingStarted);
            MyClient.StrangerTypingStopped += new EventHandler(MyClient_StrangerTypingStopped);
            MyClient.StrangerWaiting += new EventHandler(MyClient_StrangerWaiting);
            MyClient.Connect();

            while (true)
            {
                String Input = Console.ReadLine();
                if (Input == "/next")
                {
                    MyClient.Disconnect();
                    Console.Clear();
                    Console.Title = WindowTitle + "You Disconnected";
                    MyClient.Connect();
                }
                else PrintMessage(Input, MessageIssuer.Self);                
            }

            MyClient.Disconnect();
        }

        static void MyClient_StrangerWaiting(object sender, EventArgs e)
        {
            Console.Title = WindowTitle + "Waiting for Stranger...";
        }

        static void MyClient_StrangerTypingStopped(object sender, EventArgs e)
        {
            Console.Title = WindowTitle + "Currently Chatting";
        }

        static void MyClient_StrangerTypingStarted(object sender, EventArgs e)
        {
            Console.Title = WindowTitle + "Stranger is typing";
        }

        static void MyClient_StrangerSentMessage(object sender, string Message)
        {
            PrintMessage(Message, MessageIssuer.Stranger);
        }

        static void MyClient_StrangerDisconnected(object sender, EventArgs e)
        {
            Console.Title = WindowTitle + "Stranger has disconnected";
            MyClient.Disconnect();
            MyClient.Connect();
        }

        static void MyClient_StrangerConnected(object sender, EventArgs e)
        {
            Console.Clear();
            Console.Title = WindowTitle + "Currently Chatting";
        }

        static void MyClient_Error(object sender, EventArgs e)
        {
            Console.Title = WindowTitle + "Error Occured";
            MyClient.Disconnect();
            MyClient.Connect();
        }
        static void PrintMessage(String Message, MessageIssuer issuer)
        {
            if (issuer == MessageIssuer.Self)
            {
                DeletePrevConsoleLine();
                if (MyClient.SendMessage(Message))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("[YOU:] " + Message);
                    Console.ForegroundColor = ConsoleColor.White;
                }               
            }
            else if (issuer == MessageIssuer.Stranger)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("[STRANGER:] " + Message);
                Console.ForegroundColor = ConsoleColor.White;
            }
            
        }
        static void DeletePrevConsoleLine()
        {
            if (Console.CursorTop == 0) return;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
    }
}
