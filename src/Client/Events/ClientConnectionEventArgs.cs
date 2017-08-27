using System;
using System.Net;

namespace TCPUDPWrapper.Client.Events
{
    public class ClientConnectionEventArgs : EventArgs
    {
        public Object Sender { get; }
        public IPEndPoint EndPoint { get; }

        // Event arguments for client connections.
        public ClientConnectionEventArgs(Object sender, IPEndPoint endPoint)
        {
            Sender = sender;
            EndPoint = endPoint;
        }
    }
}
