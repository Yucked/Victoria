using System;
using System.Text;
using System.Runtime.InteropServices;
using Victoria.Common;

namespace Victoria.Lavalink.Decoder
{
    internal ref struct JavaBinaryReader
    {
        private readonly Span<byte> bytes;
        private int position;

        public JavaBinaryReader(Span<byte> bytes)
        {
            this.bytes = bytes;
            position = 0;
        }

        public string ReadString()
        {
            var length = Read<short>();
            int newPosition = position + length;

            if (newPosition > bytes.Length)
                Throw.Exception("String length exceeds buffer length.");

            string result = Encoding.UTF8.GetString(bytes.Slice(position, length));
            position = newPosition;

            return result;
        }

        public T Read<T>() where T : struct
        {
            T result = default;
            var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref result, 1));
            Read(bytes);
            if (BitConverter.IsLittleEndian)
                bytes.Reverse();

            return result;
        }

        private void Read(Span<byte> destination)
        {
            int newPosition = position + destination.Length;
            if (newPosition > bytes.Length)
                Throw.Exception("Destination buffer is too large.");

            bytes.Slice(position, destination.Length).CopyTo(destination);
            position = newPosition;
        }
    }
}
