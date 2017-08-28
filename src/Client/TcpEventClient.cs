using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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
    /// Tcp event based asynchronous client.
    /// </summary>
    public class TcpEventClient
    {
        /// <summary>
        /// Time to allow connection attempts to servers (in seconds).
        /// </summary>
        public int Timeout { get; set; }

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

        private int _receiveBufferSize;
        private int _sendBufferSize;

        private TcpClient _client;
        private Task _readTask;

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
        /// Constructor for an event based tcp client.
        /// </summary>
        public TcpEventClient()
        {
            _receiveBufferSize = 8192;
            _sendBufferSize = 8192;
            Timeout = 3;
        }

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

        /// <summary>
        /// Sets the buffer size for receiving messages.
        /// </summary>
        /// <param name="size">New buffer size.</param>
        public void SetReceiveBufferSize(int size)
        {
            _receiveBufferSize = size;
            if (_client != null)
                _client.ReceiveBufferSize = size;
        }

        /// <summary>
        /// Sets the buffer size for sending messages.
        /// </summary>
        /// <param name="size">New buffer size.</param>
        public void SetSendBufferSize(int size)
        {
            _sendBufferSize = size;
            if (_client != null)
                _client.SendBufferSize = size;
        }

        /// <summary>
        /// Checks whether the client is connected.
        /// </summary>
        /// <returns>True if connected, false if not connected.</returns>
        public bool IsConnected()
        {
            if (_client == null)
                return false;
            return _client.Connected;
        }

        /// <summary>
        /// Attempts to connect to an IPEndPoint. Returns true if succesful. Returns false otherwise.
        /// </summary>
        /// <param name="ep">End point to connect to.</param>
        /// <returns>Return true if connection was succesful, returns false otherwise.</returns>
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
                _readTask = new Task(Read);
                _readTask.Start();
            }

            return _client.Connected;
        }

        /// <summary>
        /// Attempt to disconnect.
        /// </summary>
        /// <returns>Returns true if disconnected succesful, returns false otherwise.</returns>
        public bool Disconnect()
        {
            if (_client == null)
                return false;

            OnDisconnect(new ClientConnectionEventArgs((IPEndPoint)_client.Client.RemoteEndPoint));

            _client.Close();
            _client = null;
            return true;
        }

        /// <summary>
        /// Asynchronously read from the server.
        /// </summary>
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

        /// <summary>
        /// Send data to the server.
        /// </summary>
        /// <param name="message">Message to send to the server.</param>
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
