using System;

namespace TCPUDPWrapper.Server.Events
{
    /// <summary>
    /// Class containing event arguments used for messages received on a server.
    /// </summary>
    public class ServerMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Client connection that sent the message.
        /// </summary>
        public ClientConnection Sender { get; }
        /// <summary>
        /// Message that was received.
        /// </summary>
        public Message Message { get; }
        /// <summary>
        /// Timestamp in Unix epoch time of when the message was received.
        /// </summary>
        public long Time { get; }

        /// <summary>
        /// Constructor for the server chat messages event info holder.
        /// </summary>
        /// <param name="sender">Client connection that sent the message.</param>
        /// <param name="message">Message that was received.</param>
        public ServerMessageEventArgs(ClientConnection sender, Message message)
        {
            Sender = sender;
            Message = message;
            Time = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}
