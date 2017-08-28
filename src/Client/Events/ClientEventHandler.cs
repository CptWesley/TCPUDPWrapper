namespace TCPUDPWrapper.Client.Events
{
    /// <summary>
    /// Abstract class that forms the base for client event listeners.
    /// </summary>
    public abstract class ClientEventHandler : IEventHandler
    {
        private TcpEventClient _client;

        /// <summary>
        /// Constructor for an event handler.
        /// </summary>
        /// <param name="client">Client that needs to be listened to.</param>
        protected ClientEventHandler(TcpEventClient client)
        {
            _client = client;

            _client.Connected += Connected;
            _client.Disconnected += Disconnected;
            _client.Received += Received;
        }

        /// <summary>
        /// Called when a client connects.
        /// </summary>
        /// <param name="sender">Object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public abstract void Connected(object sender, ClientConnectionEventArgs e);

        /// <summary>
        /// Called when a client disconnects.
        /// </summary>
        /// <param name="sender">Object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public abstract void Disconnected(object sender, ClientConnectionEventArgs e);

        /// <summary>
        /// Called when a client sends a message.
        /// </summary>
        /// <param name="sender">Object that triggered the event.</param>
        /// <param name="e">Event arguments.</param>
        public abstract void Received(object sender, ClientMessageEventArgs e);

        /// <summary>
        /// Detaches the event handler from the object it's currently attached to.
        /// </summary>
        public void Detach()
        {
            _client.Connected -= Connected;
            _client.Disconnected -= Disconnected;
            _client.Received -= Received;

            _client = null;
        }
    }
}
