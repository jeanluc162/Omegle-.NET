namespace Omegle.NET.Lib
{
    public partial class Client
    {
        public enum ConnectionStates { NotConnected, ConnectedNormal, ConnectedSpy, ConnectedSpyee}
        public ConnectionStates ConnectionState { get; protected set; }

        public enum TypingStates { Typing, NotTyping, Invalid}
        public TypingStates TypingStateClient { get; protected set; }
        public TypingStates TypingStateStranger1 { get; protected set; }
        public TypingStates TypingStateStranger2 { get; protected set; }
    }
}
