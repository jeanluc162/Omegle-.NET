using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OmegleAPI;

namespace OmegleClientWin
{
    /// <summary>
    /// Interaction logic for WindowMain.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        private readonly String WindowTitle = "Omegle - ";
        private enum MessageIssue { Self, Stranger, Stranger1, Stranger2, Info }
        private Client OmegleClient = new Client();
        public WindowMain()
        {
            this.DataContext = this;

            OmegleClient.Error += new EventHandler(OmegleClient_Error);
            OmegleClient.SpyAskedQuestion += new Client.SpyAskedQuestionHandler(OmegleClient_SpyAskedQuestion);
            OmegleClient.SpyeeDisconnected += new Client.SpyeeStatusChangedHandler(OmegleClient_SpyeeDisconnected);
            OmegleClient.SpyeeSentMessage += new Client.SpyeeSentMessageHandler(OmegleClient_SpyeeSentMessage);
            OmegleClient.SpyeeTypingStarted += new Client.SpyeeStatusChangedHandler(OmegleClient_SpyeeTypingStarted);
            OmegleClient.SpyeeTypingStopped += new Client.SpyeeStatusChangedHandler(OmegleClient_SpyeeTypingStopped);
            OmegleClient.StrangerConnected += new EventHandler(OmegleClient_StrangerConnected);
            OmegleClient.StrangerDisconnected += new EventHandler(OmegleClient_StrangerDisconnected);
            OmegleClient.StrangerSentMessage += new Client.StrangerSentMessageHandler(OmegleClient_StrangerSentMessage);
            OmegleClient.StrangerSharesInterest += new Client.StrangerSharesInterestsHandler(OmegleClient_StrangerSharesInterest);
            OmegleClient.StrangerTypingStarted += new EventHandler(OmegleClient_StrangerTypingStarted);
            OmegleClient.StrangerTypingStopped += new EventHandler(OmegleClient_StrangerTypingStopped);
            OmegleClient.StrangerWaiting += new EventHandler(OmegleClient_StrangerWaiting);

            InitializeComponent();
        }

        void OmegleClient_StrangerWaiting(object sender, EventArgs e)
        {
            SetStatus("Waiting for Stranger(s) to connect");
        }

        void OmegleClient_StrangerTypingStopped(object sender, EventArgs e)
        {
            SetStatus("Currently Chatting");
        }

        void OmegleClient_StrangerTypingStarted(object sender, EventArgs e)
        {
            SetStatus("Stranger is Typing");
        }

        void OmegleClient_StrangerSharesInterest(object sender, string[] Topics)
        {
            
        }

        void OmegleClient_StrangerSentMessage(object sender, string Message)
        {
            PostMessageToScreen(Message, MessageIssue.Stranger);
        }

        void OmegleClient_StrangerDisconnected(object sender, EventArgs e)
        {
            PostMessageToScreen("Stranger has disconnected", MessageIssue.Info);
            SetStatus("Disconnected");
            OmegleClient.Disconnect();
        }

        void OmegleClient_StrangerConnected(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() => { TbChatOutput.Text = ""; }));
            PostMessageToScreen("Stranger has connected", MessageIssue.Info);
            SetStatus("Currently Chatting");
        }

        void OmegleClient_SpyeeTypingStopped(object sender, byte StrangerID)
        {
            
        }

        void OmegleClient_SpyeeTypingStarted(object sender, byte StrangerID)
        {
            
        }

        void OmegleClient_SpyeeSentMessage(object sender, byte StrangerID, string Message)
        {
            switch (StrangerID)
            {
                case 1: PostMessageToScreen(Message, MessageIssue.Stranger1); break;
                case 2: PostMessageToScreen(Message, MessageIssue.Stranger2); break;
            }
        }

        void OmegleClient_SpyeeDisconnected(object sender, byte StrangerID)
        {
            PostMessageToScreen("Stranger " + StrangerID.ToString() + " has disconnected.", MessageIssue.Info);
        }

        void OmegleClient_SpyAskedQuestion(object sender, string Question)
        {
            PostMessageToScreen("Question to discuss: " + Question, MessageIssue.Info);
        }

        void OmegleClient_Error(object sender, EventArgs e)
        {
            MessageBox.Show("An Error has occured. Please Reconnect", "Error", MessageBoxButton.OK);
        }

        private void BtReConnect_Click(object sender, RoutedEventArgs e)
        {
            OmegleClient.Disconnect();
            if (RbChatmodeNormal.IsChecked == true)
            {
                if (TbTopics.Text.Trim().Length > 0) OmegleClient.ConnectChatInterest(TbTopics.Text.Trim().Split(new char[] { ';' }));
                OmegleClient.ConnectChat(null);
            }
            else if (RbChatmodeSpyee.IsChecked == true) OmegleClient.ConnectChatSpyee();
            else if (RbChatmodeSpy.IsChecked == true)
            {
                if (TbSpyQuestion.Text.Trim().Length > 0) OmegleClient.ConnectChatSpy(TbSpyQuestion.Text.Trim());
                else MessageBox.Show("Please enter a question!", "Error", MessageBoxButton.OK);
            }
        }

        private void BtChatSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(TbChatInput.Text);
            TbChatInput.Text = "";
        }
        private void PostMessageToScreen(String Message, MessageIssue Issue)
        {
            if (Message.Length == 0) return;
            String Prefix = "";
            if(Issue == MessageIssue.Info) Prefix = "<INFO:> ";
            else if (Issue == MessageIssue.Self) Prefix = "[YOU:] ";
            else if (Issue == MessageIssue.Stranger) Prefix = "[STRANGER:] ";
            else if (Issue == MessageIssue.Stranger1) Prefix = "[STRANGER 1:] ";
            else if (Issue == MessageIssue.Stranger2) Prefix = "[STRANGER 2:] ";
            this.Dispatcher.Invoke(new Action(() => { TbChatOutput.AppendText(Prefix + Message + "\r\n"); }));           
        }

        private void TbChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SendMessage(TbChatInput.Text);
                TbChatInput.Text = "";
            }
        }
        private void SendMessage(String Message)
        {
            if (OmegleClient.SendMessage(Message)) PostMessageToScreen(Message, MessageIssue.Self);
        }
        private void SetStatus(String Status)
        {
            this.Dispatcher.Invoke(new Action(() => { this.Title = WindowTitle + Status; }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetStatus("Disconnected");
        }
    }
}
