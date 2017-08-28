using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TCPUDPWrapper.Server.Events;
using TCPUDPWrapper.Server.Exceptions;

namespace TCPUDPWrapper.Server
{
    public delegate void ReceivedEventHandler(object sender, ServerMessageEventArgs e);
    public delegate void ConnectedEventHandler(object sender, ServerConnectionEventArgs e);
    public delegate void DisconnectedEventHandler(object sender, ServerConnectionEventArgs e);

    public class TcpEventServer
    {
        public int Port { get; set; }
        public int ReceiveBufferSize
        {
            get => _receiveBufferSize;
            set => SetReceiveBufferSize(value);
        }
        public int SendBufferSize
        {
            get => _sendBufferSize;
            set => SetSendBufferSize(value);
        }
        public bool Listening { get; private set; }

        private TcpListener _server;
        private readonly Task _listenTask;

        private List<ClientConnection> _clients;
        private int _nextId = 0;

        private int _receiveBufferSize;
        private int _sendBufferSize;

        public event ReceivedEventHandler Received;
        public event ConnectedEventHandler Connected;
        public event DisconnectedEventHandler Disconnected;

        // Constructor for a tcp event based server.
        public TcpEventServer()
        {
            _receiveBufferSize = 8192;
            _sendBufferSize = 8192;
            Listening = false;
            _listenTask = new Task(AcceptClients);
            _clients = new List<ClientConnection>();
            Port = 3000;
        }

        // Used when receiving a message from one of the clients.
        protected virtual void OnReceive(ServerMessageEventArgs e)
        {
            Received?.Invoke(this, e);
        }

        // Used when a new client connects.
        protected virtual void OnConnect(ServerConnectionEventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        // Used when a client disconnects.
        protected virtual void OnDisconnect(ServerConnectionEventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }

        // Sets the buffer size for receiving messages.
        public void SetReceiveBufferSize(int size)
        {
            _receiveBufferSize = size;
            foreach (ClientConnection client in _clients)
            {
                client.TcpClient.ReceiveBufferSize = size;
            }
        }

        // Sets the buffer size for sending messages.
        public void SetSendBufferSize(int size)
        {
            _sendBufferSize = size;
            foreach (ClientConnection client in _clients)
            {
                client.TcpClient.SendBufferSize = size;
            }
        }

        // Returns a list of all clients.
        public ClientConnection[] GetClients()
        {
            return _clients.ToArray();
        }

        // Starts the server.
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

        // Accepts new client connections.
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

        // Read thread for listening to a specific client.
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

        // Terminates a client connection.
        public void Disconnect(ClientConnection client)
        {
            OnDisconnect(new ServerConnectionEventArgs(client));
            _clients.Remove(client);
            client.TcpClient.Close();
        }

        // Send data to a client.
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
