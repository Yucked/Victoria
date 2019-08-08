using System;
using System.IO;
using System.Text;
using Victoria.Common;

namespace Victoria.Lavalink.Decoder
{
    internal sealed class JavaBinaryReader : BinaryReader
    {
        public JavaBinaryReader(Stream input) : base(input, Encoding.UTF8)
        {
        }

        public override float ReadSingle()
            => Read(4, BitConverter.ToSingle);

        public override double ReadDouble()
            => Read(8, BitConverter.ToDouble);

        public override short ReadInt16()
            => Read(2, BitConverter.ToInt16);

        public override int ReadInt32()
            => Read(4, BitConverter.ToInt32);

        public override long ReadInt64()
            => Read(8, BitConverter.ToInt64);

        public override ushort ReadUInt16()
            => Read(2, BitConverter.ToUInt16);

        public override uint ReadUInt32()
            => Read(4, BitConverter.ToUInt32);

        public override ulong ReadUInt64()
            => Read(8, BitConverter.ToUInt64);

        public override string ReadString()
        {
            var length = ReadUInt16();
            var bytes = new byte[length];
            var read = Read(bytes, 0, length);

            if (read < length)
                Throw.Exception("Bytes read were less then length.");

            return Encoding.UTF8.GetString(bytes);
        }

        private T Read<T>(int size, Func<byte[], int, T> converter) where T : struct
        {
            var bytes = GetNextBytesNativeEndian(size);
            return converter(bytes, 0);
        }

        private byte[] GetNextBytesNativeEndian(int count)
        {
            var bytes = GetNextBytes(count);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        private byte[] GetNextBytes(int count)
        {
            var buffer = new byte[count];
            var bytesRead = BaseStream.Read(buffer, 0, count);

            if (bytesRead < count)
                Throw.Exception("Unable to read stream.");

            return buffer;
        }
    }
}