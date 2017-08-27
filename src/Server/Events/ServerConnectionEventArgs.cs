using System;

namespace TCPUDPWrapper.Server.Events
{
    public class ServerConnectionEventArgs : EventArgs
    {
        public ClientConnection ClientConnection { get; }

        // Constructor for the connection events arguments.
        public ServerConnectionEventArgs(ClientConnection clientConnection)
        {
            ClientConnection = clientConnection;
        }
    }
}
