using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Victoria.Encoder {
    // Based on JavaBinaryReader struct but for writting and loosely follows the concept of a stream
    // with the main difference being that instead of overwriting the previous contents (if there are any)
    // of the current write position it just shifts the contents to the right of it, kind of like the "Insert(Range)" method from a List 
    internal ref struct JavaBinaryWriter {
        private readonly Span<byte> _bytes;
        private int _currentWritePosition;

        public int Position // Current write position
        {
            get => _currentWritePosition;
            set
            {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException(nameof(Position),
                        "An attempt was made to move the position before the beginning.");
                }

                // If we try to set the position past the length set it to the length
                // otherwise set it to the given position
                _currentWritePosition = Math.Min(value, Length);
            }
        }

        // Amount of bytes written
        public int Length { get; private set; }

        public JavaBinaryWriter(Span<byte> bytes) {
            _bytes = bytes;
            _currentWritePosition = 0;
            Length = 0;
        }

        // Write a string
        public void Write(string value) {
            var bytesToWrite = Encoding.UTF8.GetByteCount(value);

            // Write the length of the string in bytes as a short
            Write((short)bytesToWrite);

            // If we are writing to a position that already has contents, shift the existing contents to the right
            // of the current write position by the amount of bytes we need to write
            if (Position < Length) {
                _bytes.Slice(Position, Length - Position).CopyTo(_bytes.Slice(Position + bytesToWrite, Length - Position));
            }

            // Write the UTF8 encoded string to the "stream"
            Encoding.UTF8.GetBytes(value, _bytes.Slice(Position, bytesToWrite));
            Length += bytesToWrite;
            Position += bytesToWrite;
        }

        // Write to the "stream"
        // Note: Should be a primitive type!
        public void Write<T>(T value) where T : struct {
            // Get the byte contents of the value we want to write to the stream as a span
            var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1));

            // Ensure it's Big Endian
            if (BitConverter.IsLittleEndian) {
                bytes.Reverse();
            }

            Write(bytes);
        }

        public void Write(Span<byte> bytes) {
            // If we are writing to a position that already has contents, shift the existing contents to the right
            // of the current write position by the amount of bytes we need to write
            if (Position < Length) {
                _bytes.Slice(Position, Length - Position).CopyTo(_bytes.Slice(Position + bytes.Length, Length - Position));
            }

            // Write to bytes to the "stream"
            bytes.CopyTo(_bytes.Slice(Position, bytes.Length));
            Length += bytes.Length;
            Position += bytes.Length;
        }

        // Just how it works with streams essentially
        public int Seek(int offset, SeekOrigin origin) {
            return Position = origin switch {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => Position
            };
        }
    }
}