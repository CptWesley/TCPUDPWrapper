namespace TCPUDPWrapper.Server.Events
{
    /// <summary>
    /// Abstract class that forms a base for server event listeners.
    /// </summary>
    public abstract class ServerEventHandler : IEventHandler
    {
        private TcpEventServer _server;

        /// <summary>
        /// Constructor for an event handler.
        /// </summary>
        /// <param name="server">Server that needs to be listened to.</param>
        protected ServerEventHandler(TcpEventServer server)
        {
            _server = server;

            _server.Connected += Connected;
            _server.Disconnected += Disconnected;
            _server.Received += Received;
        }

        /// <summary>
        /// Called when a client connects.
        /// </summary>
        /// <param name="sender">Object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public abstract void Connected(object sender, ServerConnectionEventArgs e);

        /// <summary>
        /// Called when a client disconnects.
        /// </summary>
        /// <param name="sender">Object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public abstract void Disconnected(object sender, ServerConnectionEventArgs e);

        /// <summary>
        /// Called when a client sends a message.
        /// </summary>
        /// <param name="sender">Object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public abstract void Received(object sender, ServerMessageEventArgs e);

        /// <summary>
        /// Detaches the event handler from the object it's currently attached to.
        /// </summary>
        public void Detach()
        {
            _server.Connected -= Connected;
            _server.Disconnected -= Disconnected;
            _server.Received -= Received;

            _server = null;
        }
    }
}
