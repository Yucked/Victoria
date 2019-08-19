using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Victoria.Common;

namespace Victoria.Lavalink.Decoder
{
    internal sealed class JavaBinaryReader : IDisposable
    {
        private Stream stream;

        public JavaBinaryReader(Stream input)
        {
            stream = input;
        }

        public string ReadString()
        {
            var length = Read<short>();
            Span<byte> bytes = stackalloc byte[length];
            var read = stream.Read(bytes);

            if (read < length)
                Throw.Exception("Bytes read were less than length.");

            return Encoding.UTF8.GetString(bytes);
        }

        public T Read<T>() where T : struct
        {
            T result = default;
            GetNextBytesNativeEndian(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref result, 1)));
            return result;
        }

        private void GetNextBytesNativeEndian(Span<byte> destination)
        {
            GetNextBytes(destination);
            if (BitConverter.IsLittleEndian)
                destination.Reverse();
        }

        private void GetNextBytes(Span<byte> destination)
        {
            var bytesRead = stream.Read(destination);
            if (bytesRead < destination.Length)
                Throw.Exception("Unable to read stream.");
        }

        public void Dispose()
        {
            if(stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }
    }
}
