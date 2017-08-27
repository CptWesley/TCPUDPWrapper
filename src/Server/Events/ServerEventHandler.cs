namespace TCPUDPWrapper.Server.Events
{
    public abstract class ServerEventHandler : IEventHandler
    {
        private TcpEventServer _server;

        // Constructor for an event handler.
        protected ServerEventHandler(TcpEventServer server)
        {
            _server = server;

            _server.Connected += Connected;
            _server.Disconnected += Disconnected;
            _server.Received += Received;
        }

        // Called when a client connects.
        public abstract void Connected(object sender, ServerConnectionEventArgs e);

        // Called when a client disconnects.
        public abstract void Disconnected(object sender, ServerConnectionEventArgs e);

        // Called when a client sends a message.
        public abstract void Received(object sender, ServerMessageEventArgs e);

        // Detaches the event handler from the object it's currently attached to.
        public void Detach()
        {
            _server.Connected -= Connected;
            _server.Disconnected -= Disconnected;
            _server.Received -= Received;

            _server = null;
        }
    }
}
