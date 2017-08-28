using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TCPUDPWrapper.Client.Events;

namespace TCPUDPWrapper.Client
{
    public delegate void ClientConnectedEventHandler(object sender, ClientConnectionEventArgs e);
    public delegate void ClientDisconnectedEventHandler(object sender, ClientConnectionEventArgs e);

    public delegate void ClientReceivedEventHandler(object sender, ClientMessageEventArgs e);

    public class TcpEventClient
    {
        public int Timeout { get; set; }

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

        private int _receiveBufferSize;
        private int _sendBufferSize;

        private TcpClient _client;
        private readonly Task _readTask;

        public event ClientReceivedEventHandler Received;
        public event ClientConnectedEventHandler Connected;
        public event ClientDisconnectedEventHandler Disconnected;

        // Constructor for an event based tcp client.
        public TcpEventClient()
        {
            _receiveBufferSize = 8192;
            _sendBufferSize = 8192;
            Timeout = 3;
            _readTask = new Task(Read);
        }

        // Used when the client receives a message.
        protected virtual void OnReceive(ClientMessageEventArgs e)
        {
            Received?.Invoke(this, e);
        }

        // Used when the client connects to a server.
        protected virtual void OnConnect(ClientConnectionEventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        // Used when the client disconnects from a server.
        protected virtual void OnDisconnect(ClientConnectionEventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }

        // Sets the buffer size for receiving messages.
        public void SetReceiveBufferSize(int size)
        {
            _receiveBufferSize = size;
            if (_client != null)
                _client.ReceiveBufferSize = size;
        }

        // Sets the buffer size for sending messages.
        public void SetSendBufferSize(int size)
        {
            _sendBufferSize = size;
            if (_client != null)
                _client.SendBufferSize = size;
        }

        // Checks whether the client is connected.
        public bool IsConnected()
        {
            if (_client == null)
                return false;
            return _client.Connected;
        }

        // Attempts to connect to an IPEndPoint. Returns true if succesful. Returns false otherwise.
        public bool Connect(IPEndPoint ep)
        {
            if (IsConnected())
                return false;

            _client = new TcpClient();
            _client.SendBufferSize = _sendBufferSize;
            _client.ReceiveBufferSize = _receiveBufferSize;

            int actualTimeout = Timeout * 1000;
            if (actualTimeout < 0)
                actualTimeout = 1000;

            try
            {
                _client.ConnectAsync(ep.Address, ep.Port).Wait(actualTimeout);
            }
            catch
            {
                return false;
            }

            if (_client.Connected)
            {
                OnConnect(new ClientConnectionEventArgs((IPEndPoint)_client.Client.RemoteEndPoint));
                //Read();
                _readTask.Start();
            }

            return _client.Connected;
        }

        // Attempt to disconnect.
        public bool Disconnect()
        {
            if (_client == null)
                return false;

            OnDisconnect(new ClientConnectionEventArgs((IPEndPoint)_client.Client.RemoteEndPoint));

            _client.GetStream().Close();
            _client.Close();
            _client.Client.Dispose();
            _client.GetStream().Dispose();
            _client = null;
            return true;
        }

        // Asynchronously read from the server.
        private void Read()
        {
            while (IsConnected())
            {
                List<byte> msg = new List<byte>();
                bool ended = false;
                do
                {
                    byte[] buffer = new byte[_receiveBufferSize];
                    int received;
                    try
                    {
                        received = _client.GetStream().Read(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        Disconnect();
                        return;
                    }

                    if (received == 0)
                    {
                        Disconnect();
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
                OnReceive(new ClientMessageEventArgs(new Message(msg.ToArray())));
            }
        }

        // Send data to the server.
        public void Send(Message message)
        {
            if (!IsConnected())
                return;

            byte[] buffer = new byte[message.Bytes.Length+1];
            Array.Copy(message.Bytes, buffer, message.Bytes.Length);
            buffer[buffer.Length - 1] = Message.ETX;

            try
            {
                _client.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
            catch
            {
                // Do nothing (for now).
            }
        }
    }
}
