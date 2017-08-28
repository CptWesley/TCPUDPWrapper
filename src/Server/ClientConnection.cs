using System;
using System.Net;
using System.Net.Sockets;

namespace TCPUDPWrapper.Server
{
    /// <summary>
    /// Class containing info on a connection established between a server and a client.
    /// </summary>
    public class ClientConnection
    {
        /// <summary>
        /// Unique ID of the connection.
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// TcpClient used for sending/receiving messages to the client.
        /// </summary>
        public TcpClient TcpClient { get; }
        /// <summary>
        /// Ip of the client.
        /// </summary>
        public IPAddress Ip => ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address;
        /// <summary>
        /// Timestamp of when the client connected in Unix Epoch time.
        /// </summary>
        public long Joined { get; }

        /// <summary>
        /// Constructor for client connection info.
        /// </summary>
        /// <param name="client">TcpClient used for communication with the client.</param>
        /// <param name="id">Unique Id of the connection.</param>
        public ClientConnection(TcpClient client, int id)
        {
            Id = id;
            TcpClient = client;
            Joined = (long) (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}
