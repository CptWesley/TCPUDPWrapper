using System;

namespace TCPUDPWrapper.Server.Events
{
    /// <summary>
    /// Class containing the event arguments used for events regarding server connections.
    /// </summary>
    public class ServerConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Client connection this event relates to.
        /// </summary>
        public ClientConnection ClientConnection { get; }

        /// <summary>
        /// Constructor for the connection events arguments.
        /// </summary>
        /// <param name="clientConnection">Client connection that this event relates to.</param>
        public ServerConnectionEventArgs(ClientConnection clientConnection)
        {
            ClientConnection = clientConnection;
        }
    }
}
