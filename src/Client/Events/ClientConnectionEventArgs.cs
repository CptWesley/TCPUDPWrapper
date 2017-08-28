using System;
using System.Net;

namespace TCPUDPWrapper.Client.Events
{
    /// <summary>
    /// Event arguments holder for client connection events.
    /// </summary>
    public class ClientConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// EndPoint the client is/was connected to.
        /// </summary>
        public IPEndPoint EndPoint { get; }

        /// <summary>
        /// Event arguments for client connections.
        /// </summary>
        /// <param name="endPoint">EndPoint the client is/was connected to.</param>
        public ClientConnectionEventArgs(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }
    }
}
