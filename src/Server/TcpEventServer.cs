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
        public int BufferSize { get; set; }
        public bool Listening { get; private set; }

        private TcpListener _server;
        private readonly Task _listenTask;

        private List<ClientConnection> _clients;
        private int _nextId = 0;

        public event ReceivedEventHandler Received;
        public event ConnectedEventHandler Connected;
        public event DisconnectedEventHandler Disconnected;

        // Constructor for a tcp event based server.
        public TcpEventServer()
        {
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
                new Task(() => ReadFromClient(clientConnection)).Start();
            }
        }

        // Read thread for listening to a specific client.
        private void ReadFromClient(ClientConnection client)
        {
            while (Listening)
            {
                byte[] message;
                try
                {
                    message = Read(client.TcpClient);
                }
                catch (ClientDisconnectedException e)
                {
                    OnDisconnect(new ServerConnectionEventArgs(client));
                    _clients.Remove(client);
                    return;
                }

                if (message.Length == 0)
                {
                    OnDisconnect(new ServerConnectionEventArgs(client));
                    _clients.Remove(client);
                    return;
                }

                OnReceive(new ServerMessageEventArgs(client, new Message(message)));
            }
        }

        // Reads bytes from a TcpClient.
        private byte[] Read(TcpClient client)
        {
            if (!Listening)
                return new byte[0];

            byte[] buffer = new byte[1024];
            try
            {
                client.GetStream().Read(buffer, 0, buffer.Length);
            }
            catch
            {
                throw new ClientDisconnectedException();
            }

            byte val = 255;
            int index = 0;
            while (val != 0)
            {
                val = buffer[index];
                ++index;
            }

            return buffer.Take(index - 1).ToArray();
        }

        // Send data to a client.
        public void Send(ClientConnection client, Message message)
        {
            if (!Listening)
                return;

            try
            {
                client.TcpClient.GetStream().WriteAsync(message.Bytes, 0, message.Bytes.Length);
            }
            catch
            {
                // Do nothing (for now).
            }
        }
    }
}
