using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Victoria.Player.Decoder {
    internal ref struct JavaBinaryReader {
        private readonly Span<byte> _bytes;
        private int _position;

        public JavaBinaryReader(Span<byte> bytes) {
            _bytes = bytes;
            _position = 0;
        }

        public string ReadString() {
            var length = Read<short>();
            var newPosition = _position + length;

            if (newPosition > _bytes.Length) {
                throw new Exception("String length exceeds buffer length.");
            }

            var result = Encoding.UTF8.GetString(_bytes.Slice(_position, length));
            _position = newPosition;

            return result;
        }

        public T Read<T>() where T : struct {
            T result = default;
            var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref result, 1));
            Read(bytes);

            if (BitConverter.IsLittleEndian) {
                bytes.Reverse();
            }

            return result;
        }

        private void Read(Span<byte> destination) {
            var newPosition = _position + destination.Length;

            if (newPosition > _bytes.Length) {
                throw new Exception("Destination buffer is too large.");
            }

            _bytes
                .Slice(_position, destination.Length)
                .CopyTo(destination);

            _position = newPosition;
        }
    }
}