using System;

namespace TCPUDPWrapper.events
{
    public class ClientMessageEventArgs : EventArgs
    {
        public Object Sender { get; }
        public Message Message { get; }

        // Event arguments for messages received by clients.
        public ClientMessageEventArgs(Object sender, Message message)
        {
            Sender = sender;
            Message = message;
        }
    }
}
