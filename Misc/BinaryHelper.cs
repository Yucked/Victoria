using System;
using System.IO;
using System.Text;

namespace Victoria.Misc
{
    // Taken From: https://github.com/DSharpPlus/DSharpPlus/blob/master/DSharpPlus.Lavalink/LavalinkUtil.cs
    // // Coz I'm not smart enough for this. Thanks Emzi.
    internal sealed class BinaryHelper : BinaryReader
    {
        private static readonly Encoding Utf8 = new UTF8Encoding();

        public BinaryHelper(Stream input) : base(input, Utf8)
        {
        }

        public string ReadJavaUtf8()
        {
            var length = ReadUInt16(); // string size in bytes
            var bytes = new byte[length];
            var amountRead = Read(bytes, 0, length);
            if (amountRead < length)
                throw new InvalidDataException("EOS unexpected");

            var output = new char[length];
            var strlen = 0;

            // i'm gonna blindly assume here that the javadocs had the correct endianness

            for (var i = 0; i < length; i++)
            {
                var value1 = bytes[i];
                if ((value1 & 0b10000000) == 0) // highest bit 1 is false
                {
                    output[strlen++] = (char) value1;
                    continue;
                }

                // remember to skip one byte for every extra byte
                var value2 = bytes[++i];
                if ((value1 & 0b00100000) == 0 && // highest bit 3 is false
                    (value1 & 0b11000000) != 0 && // highest bit 1 and 2 are true
                    (value2 & 0b01000000) == 0 && // highest bit 2 is false
                    (value2 & 0b10000000) != 0) //   highest bit 1 is true
                {
                    var value1Chop = (value1 & 0b00011111) << 6;
                    var value2Chop = value2 & 0b00111111;
                    output[strlen++] = (char) (value1Chop | value2Chop);
                    continue;
                }

                var value3 = bytes[++i];
                if ((value1 & 0b00010000) != 0 || (value1 & 0b11100000) == 0 || (value2 & 0b01000000) != 0 ||
                    (value2 & 0b10000000) == 0 || (value3 & 0b01000000) != 0 || (value3 & 0b10000000) == 0) continue;
                {
                    var value1Chop = (value1 & 0b00001111) << 12;
                    var value2Chop = (value2 & 0b00111111) << 6;
                    var value3Chop = (value3 & 0b00111111);
                    output[strlen++] = (char) (value1Chop | value2Chop | value3Chop);
                }
            }

            return new string(output, 0, strlen);
        }

        // https://github.com/sedmelluq/lavaplayer/blob/b0c536098c4f92e6d03b00f19221021f8f50b19b/main/src/main/java/com/sedmelluq/discord/lavaplayer/tools/DataFormatTools.java#L114-L125
        public string ReadNullableString()
        {
            return ReadBoolean() ? ReadJavaUtf8() : null;
        }

        // from https://github.com/Zoltu/Zoltu.EndianAwareBinaryReaderWriter under CC0
        public override float ReadSingle() => Read(4, BitConverter.ToSingle);

        public override double ReadDouble() => Read(8, BitConverter.ToDouble);

        public override short ReadInt16() => Read(2, BitConverter.ToInt16);

        public override int ReadInt32() => Read(4, BitConverter.ToInt32);

        public override long ReadInt64() => Read(8, BitConverter.ToInt64);

        public override ushort ReadUInt16() => Read(2, BitConverter.ToUInt16);

        public override uint ReadUInt32() => Read(4, BitConverter.ToUInt32);

        public override ulong ReadUInt64() => Read(8, BitConverter.ToUInt64);

        private T Read<T>(int size, Func<byte[], int, T> converter) where T : struct
        {
            //Contract.Requires(size >= 0);
            //Contract.Requires(converter != null);

            var bytes = GetNextBytesNativeEndian(size);
            return converter(bytes, 0);
        }

        private byte[] GetNextBytesNativeEndian(int count)
        {
            //Contract.Requires(count >= 0);
            //Contract.Ensures(Contract.Result<Byte[]>() != null);
            //Contract.Ensures(Contract.Result<Byte[]>().Length == count);

            var bytes = GetNextBytes(count);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        private byte[] GetNextBytes(int count)
        {
            //Contract.Requires(count >= 0);
            //Contract.Ensures(Contract.Result<Byte[]>() != null);
            //Contract.Ensures(Contract.Result<Byte[]>().Length == count);

            var buffer = new byte[count];
            var bytesRead = BaseStream.Read(buffer, 0, count);

            if (bytesRead != count)
                throw new EndOfStreamException();

            return buffer;
        }
    }
}