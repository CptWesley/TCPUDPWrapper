using System;

namespace TCPUDPWrapper.Client.Events
{
    /// <summary>
    /// Event arguments holder for client message events.
    /// </summary>
    public class ClientMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Message that was received.
        /// </summary>
        public Message Message { get; }

        /// <summary>
        /// Event arguments for messages received by clients.
        /// </summary>
        /// <param name="message">Message that was received.</param>
        public ClientMessageEventArgs(Message message)
        {
            Message = message;
        }
    }
}
