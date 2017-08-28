using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TCPUDPWrapper.Server.Events;

namespace TCPUDPWrapper.Server
{
    /// <summary>
    /// Tcp event based asynchronous server.
    /// </summary>
    public class TcpEventServer : EventServer
    {
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

        private TcpListener _server;
        private readonly Task _listenTask;

        private int _receiveBufferSize;
        private int _sendBufferSize;

        /// <summary>
        /// Constructor for a tcp event based server.
        /// </summary>
        public TcpEventServer()
        {
            _receiveBufferSize = 8192;
            _sendBufferSize = 8192;
            _listenTask = new Task(AcceptClients);
        }

        /// <summary>
        /// Sets the buffer size for receiving messages.
        /// </summary>
        /// <param name="size">New buffer size.</param>
        public void SetReceiveBufferSize(int size)
        {
            _receiveBufferSize = size;
            foreach (ClientConnection client in Clients)
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
            foreach (ClientConnection client in Clients)
            {
                client.TcpClient.SendBufferSize = size;
            }
        }

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <returns>True if started succesfully, false otherwise.</returns>
        public override bool Start()
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

            Clients = new List<ClientConnection>();
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
                ClientConnection clientConnection = new ClientConnection(client, NextId());

                Clients.Add(clientConnection);

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
        public override void Disconnect(ClientConnection client)
        {
            OnDisconnect(new ServerConnectionEventArgs(client));
            Clients.Remove(client);
            client.TcpClient.Close();
        }

        /// <summary>
        /// Send data to a client.
        /// </summary>
        /// <param name="client">Client to send to.</param>
        /// <param name="message">Message to send.</param>
        public override void Send(ClientConnection client, Message message)
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
