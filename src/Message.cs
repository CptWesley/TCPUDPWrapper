using System.Collections.Generic;
using System.Text;

namespace TCPUDPWrapper
{
    public class Message
    {
        public const byte Null = 0;
        public const byte Filler = 255;
        public const byte ETX = 3;

        public byte[] Bytes { get; }

        // Constructor of a message.
        public Message(byte[] bytes)
        {
            Bytes = bytes;
        }

        // Constructor of a message using a single string.
        public Message(string message)
        {
            Bytes = new byte[message.Length];
            for (int i = 0; i < message.Length; ++i)
                Bytes[i] = (byte)message[i];
        }

        // Constructor of a message using an array of strings.
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
                    bytes.Add(Filler);
            }
            Bytes = bytes.ToArray();
        }

        // Get all parts of the message.
        public string[] GetParts()
        {
            List<string> parts = new List<string>();
            parts.Add(Encoding.UTF8.GetString(Bytes));

            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < Bytes.Length; ++i)
            {
                if (Bytes[i] != Filler && Bytes[i] != Null)
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

        // Returns the message as a string.
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
