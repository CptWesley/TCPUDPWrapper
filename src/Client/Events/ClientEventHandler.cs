namespace TCPUDPWrapper.Client.Events
{
    public abstract class ClientEventHandler : IEventHandler
    {
        private TcpEventClient _client;

        // Constructor for an event handler.
        protected ClientEventHandler(TcpEventClient client)
        {
            _client = client;

            _client.Connected += Connected;
            _client.Disconnected += Disconnected;
            _client.Received += Received;
        }

        // Called when a client connects.
        public abstract void Connected(object sender, ClientConnectionEventArgs e);

        // Called when a client disconnects.
        public abstract void Disconnected(object sender, ClientConnectionEventArgs e);

        // Called when a client sends a message.
        public abstract void Received(object sender, ClientMessageEventArgs e);

        // Detaches the event handler from the object it's currently attached to.
        public void Detach()
        {
            _client.Connected -= Connected;
            _client.Disconnected -= Disconnected;
            _client.Received -= Received;

            _client = null;
        }
    }
}
