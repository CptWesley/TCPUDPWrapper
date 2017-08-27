using System;

namespace TCPUDPWrapper.Client.Events
{
    public class ClientMessageEventArgs : EventArgs
    {
        public Message Message { get; }

        // Event arguments for messages received by clients.
        public ClientMessageEventArgs(Message message)
        {
            Message = message;
        }
    }
}
