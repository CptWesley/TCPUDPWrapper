using System;

namespace TCPUDPWrapper.Server.Exceptions
{
    public class ClientDisconnectedException : Exception
    {
        public ClientDisconnectedException()
        {
        }

        public ClientDisconnectedException(string message) : base(message)
        {
        }

        public ClientDisconnectedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
