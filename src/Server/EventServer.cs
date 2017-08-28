using System.Collections.Generic;
using TCPUDPWrapper.Server.Events;

namespace TCPUDPWrapper.Server
{
    /// /// <summary>
    /// EventHandler for an event that is fired when a client receives a message from the server.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void ReceivedEventHandler(object sender, ServerMessageEventArgs e);
    /// <summary>
    /// EventHandler for an event that is fired when a client connects to a server.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void ConnectedEventHandler(object sender, ServerConnectionEventArgs e);
    /// <summary>
    /// EventHandler for an event that is fired when a client disconnects from a server.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void DisconnectedEventHandler(object sender, ServerConnectionEventArgs e);


    /// <summary>
    /// Abstract class base for event based asynchronous servers.
    /// </summary>
    public abstract class EventServer
    {
        /// <summary>
        /// Port the server is running on.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Checks if the server is currently listening or not.
        /// </summary>
        public bool Listening { get; protected set; }

        /// <summary>
        /// Current client connections on this server.
        /// </summary>
        protected List<ClientConnection> Clients;

        /// <summary>
        /// Event that is fired when a client receives a message from the server.
        /// </summary>
        public event ReceivedEventHandler Received;
        /// <summary>
        /// Event that is fired when a client connects to a server.
        /// </summary>
        public event ConnectedEventHandler Connected;
        /// <summary>
        /// Event that is fired when a client disconnects from a server.
        /// </summary>
        public event DisconnectedEventHandler Disconnected;

        private int _nextId = 0;

        /// <summary>
        /// Constructor for an event based server.
        /// </summary>
        protected EventServer()
        {
            Clients = new List<ClientConnection>();
            Listening = false;
            Port = 3000;
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns>True if started succesfully, false otherwise.</returns>
        public abstract bool Start();

        /// <summary>
        /// Terminates a client connection.
        /// </summary>
        /// <param name="client">Client to disconnect with.</param>
        public abstract void Disconnect(ClientConnection client);

        /// <summary>
        /// Send data to a client.
        /// </summary>
        /// <param name="client">Client to send to.</param>
        /// <param name="message">Message to send.</param>
        public abstract void Send(ClientConnection client, Message message);

        /// <summary>
        /// Used when receiving a message from one of the clients.
        /// </summary>
        /// <param name="e">Event arguments containing the message that was received.</param>
        protected virtual void OnReceive(ServerMessageEventArgs e)
        {
            Received?.Invoke(this, e);
        }

        /// <summary>
        /// Used when a new client connects.
        /// </summary>
        /// <param name="e">Event arguments containing the client info that connected.</param>
        protected virtual void OnConnect(ServerConnectionEventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        /// <summary>
        /// Used when a client disconnects.
        /// </summary>
        /// <param name="e">Event arguments containing the client info that disconnected.</param>
        protected virtual void OnDisconnect(ServerConnectionEventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }

        /// <summary>
        /// Returns a list of all clients.
        /// </summary>
        /// <returns>An array containing all client connections.</returns>
        public ClientConnection[] GetClients()
        {
            return Clients.ToArray();
        }

        /// <summary>
        /// Gets the next unique client connection ID and increments the value.
        /// </summary>
        /// <returns>The next unique client connection ID.</returns>
        protected int NextId()
        {
            int val = _nextId;
            ++_nextId;
            return val;
        }

    }
}
