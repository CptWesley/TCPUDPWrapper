using System;
using System.Net;

namespace TCPUDPWrapper.Client.Events
{
    public class ClientConnectionEventArgs : EventArgs
    {
        public IPEndPoint EndPoint { get; }

        // Event arguments for client connections.
        public ClientConnectionEventArgs(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }
    }
}
