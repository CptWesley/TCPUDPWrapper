using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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
    /// Tcp event based asynchronous server.
    /// </summary>
    public class TcpEventServer
    {
        /// <summary>
        /// Port the server is running on.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Buffer size of received messages.
        /// </summary>
        public int ReceiveBufferSize
        {
            get => _receiveBufferSize;
            set => SetReceiveBufferSize(value);
        }
        /// <summary>
        /// Buffer size of outgoing messages.
        /// </summary>
        public int SendBufferSize
        {
            get => _sendBufferSize;
            set => SetSendBufferSize(value);
        }
        /// <summary>
        /// Checks if the server is currently listening or not.
        /// </summary>
        public bool Listening { get; private set; }

        private TcpListener _server;
        private readonly Task _listenTask;

        private List<ClientConnection> _clients;
        private int _nextId = 0;

        private int _receiveBufferSize;
        private int _sendBufferSize;

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

        /// <summary>
        /// Constructor for a tcp event based server.
        /// </summary>
        public TcpEventServer()
        {
            _receiveBufferSize = 8192;
            _sendBufferSize = 8192;
            Listening = false;
            _listenTask = new Task(AcceptClients);
            _clients = new List<ClientConnection>();
            Port = 3000;
        }

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
        /// Sets the buffer size for receiving messages.
        /// </summary>
        /// <param name="size">New buffer size.</param>
        public void SetReceiveBufferSize(int size)
        {
            _receiveBufferSize = size;
            foreach (ClientConnection client in _clients)
            {
                client.TcpClient.ReceiveBufferSize = size;
            }
        }

        /// <summary>
        /// Sets the buffer size for sending messages.
        /// </summary>
        /// <param name="size">New buffer size.</param>
        public void SetSendBufferSize(int size)
        {
            _sendBufferSize = size;
            foreach (ClientConnection client in _clients)
            {
                client.TcpClient.SendBufferSize = size;
            }
        }

        /// <summary>
        /// Returns a list of all clients.
        /// </summary>
        /// <returns>An array containing all client connections.</returns>
        public ClientConnection[] GetClients()
        {
            return _clients.ToArray();
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns>True if started succesfully, false otherwise.</returns>
        public bool Start()
        {
            if (Listening)
                return false;

            _server = new TcpListener(IPAddress.Any, Port);

            try
            {
                _server.Start();
            }
            catch (SocketException e)
            {
                return false;
            }

            _clients = new List<ClientConnection>();
            _listenTask.Start();
            Listening = true;

            return true;
        }

        /// <summary>
        /// Accepts new client connections.
        /// </summary>
        private void AcceptClients()
        {
            while (Listening)
            {
                TcpClient client = _server.AcceptTcpClient();
                ClientConnection clientConnection = new ClientConnection(client, _nextId);
                ++_nextId;

                _clients.Add(clientConnection);

                OnConnect(new ServerConnectionEventArgs(clientConnection));
                new Task(() => Read(clientConnection)).Start();
            }
        }

        /// <summary>
        /// Read thread for listening to a specific client.
        /// </summary>
        /// <param name="client">Client to read from.</param>
        private void Read(ClientConnection client)
        {
            while (Listening)
            {
                List<byte> msg = new List<byte>();
                bool ended = false;
                do
                {
                    byte[] buffer = new byte[ReceiveBufferSize];
                    int received;
                    try
                    {
                        received = client.TcpClient.GetStream().Read(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        Disconnect(client);
                        return;
                    }

                    if (received == 0)
                    {
                        Disconnect(client);
                        return;
                    }

                    for (int i = 0; i < received; ++i)
                    {
                        if (buffer[i] == Message.ETX)
                        {
                            ended = true;
                            break;
                        }
                        msg.Add(buffer[i]);
                    }

                } while (!ended);

                OnReceive(new ServerMessageEventArgs(client, new Message(msg.ToArray())));
            }
        }

        /// <summary>
        /// Terminates a client connection.
        /// </summary>
        /// <param name="client">Client to disconnect with.</param>
        public void Disconnect(ClientConnection client)
        {
            OnDisconnect(new ServerConnectionEventArgs(client));
            _clients.Remove(client);
            client.TcpClient.Close();
        }

        /// <summary>
        /// Send data to a client.
        /// </summary>
        /// <param name="client">Client to send to.</param>
        /// <param name="message">Message to send.</param>
        public void Send(ClientConnection client, Message message)
        {
            if (!Listening)
                return;

            byte[] buffer = new byte[message.Bytes.Length + 1];
            Array.Copy(message.Bytes, buffer, message.Bytes.Length);
            buffer[buffer.Length - 1] = Message.ETX;

            try
            {
                client.TcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
            catch
            {
                // Do nothing (for now).
            }
        }
    }
}
