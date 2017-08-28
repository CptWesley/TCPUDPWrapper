using System.Net;
using TCPUDPWrapper.Client.Events;

namespace TCPUDPWrapper.Client
{
    /// <summary>
    /// EventHandler for an event that is fired when a client connects to a server.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void ClientConnectedEventHandler(object sender, ClientConnectionEventArgs e);
    /// <summary>
    /// EventHandler for an event that is fired when a client disconnects from a server.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void ClientDisconnectedEventHandler(object sender, ClientConnectionEventArgs e);
    /// <summary>
    /// EventHandler for an event that is fired when a client receives a message from the server.
    /// </summary>
    /// <param name="sender">Sender of the event.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void ClientReceivedEventHandler(object sender, ClientMessageEventArgs e);

    /// <summary>
    /// Abstract class base for event based asynchronous clients.
    /// </summary>
    public abstract class EventClient
    {
        /// <summary>
        /// Event that is fired when a client receives a message from the server.
        /// </summary>
        public event ClientReceivedEventHandler Received;
        /// <summary>
        /// Event that is fired when a client connects to a server.
        /// </summary>
        public event ClientConnectedEventHandler Connected;
        /// <summary>
        /// Event that is fired when a client disconnects from a server.
        /// </summary>
        public event ClientDisconnectedEventHandler Disconnected;

        /// <summary>
        /// Checks whether the client is connected.
        /// </summary>
        /// <returns>True if connected, false if not connected.</returns>
        public abstract bool IsConnected();

        /// <summary>
        /// Attempts to connect to an IPEndPoint. Returns true if succesful. Returns false otherwise.
        /// </summary>
        /// <param name="ep">End point to connect to.</param>
        /// <returns>Return true if connection was succesful, returns false otherwise.</returns>
        public abstract bool Connect(IPEndPoint ep);

        /// <summary>
        /// Attempt to disconnect.
        /// </summary>
        /// <returns>Returns true if disconnected succesful, returns false otherwise.</returns>
        public abstract bool Disconnect();

        /// <summary>
        /// Send data to the server.
        /// </summary>
        /// <param name="message">Message to send to the server.</param>
        public abstract void Send(Message message);

        /// <summary>
        /// Used when the client receives a message.
        /// </summary>
        /// <param name="e">Event arguments containing the message that was received.</param>
        protected virtual void OnReceive(ClientMessageEventArgs e)
        {
            Received?.Invoke(this, e);
        }

        /// <summary>
        /// Used when the client connects to a server.
        /// </summary>
        /// <param name="e">Event arguments containing the server info that was connected to.</param>
        protected virtual void OnConnect(ClientConnectionEventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        /// <summary>
        /// Used when the client disconnects from a server.
        /// </summary>
        /// <param name="e">Event arguments containing the server info that we disconnected from.</param>
        protected virtual void OnDisconnect(ClientConnectionEventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }
    }
}
