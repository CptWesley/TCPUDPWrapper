using System;

namespace TCPUDPWrapper.Server.Events
{
    public class ServerMessageEventArgs : EventArgs
    {
        public ClientConnection Sender { get; }
        public Message Message { get; }
        public long Time { get; }

        // Constructor for the server chat messages event info holder.
        public ServerMessageEventArgs(ClientConnection sender, Message message)
        {
            Sender = sender;
            Message = message;
            Time = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}
