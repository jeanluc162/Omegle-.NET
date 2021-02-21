using System;

namespace Omegle.NET.Lib
{
    public partial class Client
    {
        protected class AlreadyConnectedException : Exception
        {
            public AlreadyConnectedException(String ClientID) : base()
            {
                this.ClientID = ClientID;
            }
            public String ClientID { get; private set; }
        }
        protected class NotConnectedException : Exception
        {

        }
    }  
}
