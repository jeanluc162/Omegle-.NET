using Omegle.NET.Lib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Omegle.NET.Examples.GUI
{
    class MainWindowVM : Client, INotifyPropertyChanged
    {
        public class ChatEntry
        {
            public String Message { get; set; }
            public enum Senders { Me, Stranger, Stranger_1, Stranger_2}
            public Senders Sender { get; set; }
            public Brush DisplayColor
            {
                get
                {
                    switch(Sender)
                    {
                        case Senders.Me or Senders.Stranger_2:
                            return Brushes.Red;
                        case Senders.Stranger or Senders.Stranger_1:
                            return Brushes.Blue;
                        default:
                            return Brushes.Green;
                    }
                }
            }
        }
        public class ReConnectCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;
            private MainWindowVM _mwv;
            public ReConnectCommand(MainWindowVM mwv)
            {
                _mwv = mwv;
            }
            public bool CanExecute(object parameter)
            {
                return true;
            }

            public async void Execute(object parameter)
            {
                await _mwv.ConnectAsync(_mwv.SelectedLanguage);
            }
        }
        public ICommand ReConnect { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<ChatEntry> ChatEntries { get; private set; }
        public String SelectedLanguage { get; set; }
        public Boolean Unmoderated { get; set; }
        public String Topics { get; set; }
        public String[] AvailableLanguages
        {
            get
            {
                return CountryCodes;
            }
        }
        public MainWindowVM()
        {
            ChatEntries = new ObservableCollection<ChatEntry>();
            SelectedLanguage = "en";
            ReConnect = new ReConnectCommand(this);
        }
        protected override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            ChatEntries.Clear();
        }
        protected override async Task OnReceivedMessageAsync(string Message, int? PeerID = null)
        {
            await base.OnReceivedMessageAsync(Message, PeerID);
            ChatEntries.Add(new ChatEntry { Message = Message, Sender = ChatEntry.Senders.Stranger });
        }
    }
}
