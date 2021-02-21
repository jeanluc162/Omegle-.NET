using Omegle.NET.Lib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace Omegle.NET.Examples.GUI
{
    class MainWindowVM : Client, INotifyPropertyChanged
    {
        public class ChatEntry
        {
            public String Message { get; set; }
            public enum Senders { Me, Stranger, Stranger_1, Stranger_2}
            public Senders Sender { get; set; }
            public KnownColor DisplayColor
            {
                get
                {
                    switch(Sender)
                    {
                        case Senders.Me or Senders.Stranger_2:
                            return KnownColor.Red;
                        case Senders.Stranger or Senders.Stranger_1:
                            return KnownColor.Blue;
                        default:
                            return KnownColor.Green;
                    }
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<ChatEntry> ChatEntries { get; private set; }
        public MainWindowVM()
        {
            ChatEntries = new ObservableCollection<ChatEntry>();
            ChatEntries.Add(new ChatEntry { Message = "wd", Sender = ChatEntry.Senders.Stranger });
        }
    }
}
