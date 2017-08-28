using System.Collections.Generic;
using System.Text;

namespace TCPUDPWrapper
{
    /// <summary>
    /// Class containing a message that was received or needs to be send between tcp/udp servers/clients.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Byte representing null.
        /// </summary>
        public const byte Null = 0;
        /// <summary>
        /// Byte representing a seperation of parts.
        /// </summary>
        public const byte Seperator = 255;
        /// <summary>
        /// Byte representing an end-of-line character.
        /// </summary>
        public const byte ETX = 3;

        /// <summary>
        /// Bytes representing a message.
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// Constructor of a message using a byte array.
        /// </summary>
        /// <param name="bytes">Bytes representing a message.</param>
        public Message(byte[] bytes)
        {
            Bytes = bytes;
        }

        /// <summary>
        /// Constructor of a message using a single string.
        /// </summary>
        /// <param name="message">String representing a message.</param>
        public Message(string message)
        {
            Bytes = new byte[message.Length];
            for (int i = 0; i < message.Length; ++i)
                Bytes[i] = (byte)message[i];
        }

        /// <summary>
        /// Constructor of a message using an array of strings.
        /// </summary>
        /// <param name="parts">Array of strings to be send. Each string will be send, seperated with a Filler character (255).</param>
        public Message(string[] parts)
        {
            List<byte> bytes = new List<byte>(3);
            for (int i = 0; i < parts.Length; ++i)
            {
                foreach (char c in parts[i])
                {
                    bytes.Add((byte)c);
                }
                if (i != parts.Length-1)
                    bytes.Add(Seperator);
            }
            Bytes = bytes.ToArray();
        }

        /// <summary>
        /// Get all parts of the message.
        /// </summary>
        /// <returns>An array containing all parts of a message.</returns>
        public string[] GetParts()
        {
            List<string> parts = new List<string>();
            parts.Add(Encoding.UTF8.GetString(Bytes));

            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < Bytes.Length; ++i)
            {
                if (Bytes[i] != Seperator && Bytes[i] != Null)
                    buffer.Append((char)Bytes[i]);
                else
                {
                    parts.Add(buffer.ToString());
                    buffer.Clear();
                }
            }

            // Add last part.
            if (buffer.Length > 0)
                parts.Add(buffer.ToString());

            return parts.ToArray();
        }

        /// <summary>
        /// Returns the message as a string.
        /// </summary>
        /// <returns>A stringified version of the bytes contained in this message.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in Bytes)
            {
                sb.Append((char) b);
            }

            return sb.ToString();
        }
    }
}
