using System;
using System.IO;
using System.Text;

namespace LTC2.Shared.Utils.Utils
{
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int b1 = ioStream.ReadByte();
            if (b1 == -1) throw new EndOfStreamException("Pipe closed.");

            int b2 = ioStream.ReadByte();
            if (b2 == -1) throw new EndOfStreamException("Pipe closed.");

            int len = b1 * 256 + b2;
            byte[] inBuffer = new byte[len];

            int totalRead = 0;
            while (totalRead < len)
            {
                int read = ioStream.Read(inBuffer, totalRead, len - totalRead);
                if (read == 0) throw new EndOfStreamException("Pipe closed during read.");
                totalRead += read;
            }

            return streamEncoding.GetString(inBuffer);
        }

        private readonly object writeLock = new object();

        public int WriteString(string outString)
        {
            lock (writeLock)
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                int len = outBuffer.Length;
                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }
                ioStream.WriteByte((byte)(len / 256));
                ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();

                return outBuffer.Length + 2;
            }
        }
    }
}
