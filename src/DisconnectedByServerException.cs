using System;

namespace TCPUDPWrapper
{
    public class DisconnectedByServerException : Exception
    {
        public DisconnectedByServerException()
        {
        }

        public DisconnectedByServerException(string message) : base(message)
        {
        }

        public DisconnectedByServerException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
