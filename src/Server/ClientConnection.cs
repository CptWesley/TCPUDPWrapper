using System;
using System.Net;
using System.Net.Sockets;

namespace TCPUDPWrapper.Server
{
    public class ClientConnection
    {
        public int Id { get; }
        public TcpClient TcpClient { get; }
        public IPAddress Ip => ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address;
        public long Joined { get; }

        // Constructor for client connection info.
        public ClientConnection(TcpClient client, int id)
        {
            Id = id;
            TcpClient = client;
            Joined = (long) (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}
