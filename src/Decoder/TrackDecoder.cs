using System;
using System.Buffers.Text;
using System.Text;

namespace Victoria.Decoder {
    /// <summary>
    /// Helper class for decoding Lavalink's <see cref="LavaTrack"/> hash.
    /// </summary>
    public readonly struct TrackDecoder {
        /// <summary>
        ///     Decodes the hash for the specified track.
        /// </summary>
        /// <param name="hash">Track's hash.</param>
        /// <returns>
        ///     <see cref="LavaTrack" />
        /// </returns>
        public static LavaTrack Decode(string hash) {
            Span<byte> hashBuffer = stackalloc byte[hash.Length];
            Encoding.ASCII.GetBytes(hash, hashBuffer);
            Base64.DecodeFromUtf8InPlace(hashBuffer, out var bytesWritten);
            var javaReader = new JavaBinaryReader(hashBuffer.Slice(0, bytesWritten));

            // Reading header
            var header = javaReader.Read<int>();
            var flags = (int) ((header & 0xC0000000L) >> 30);
            var hasVersion = (flags & 1) != 0;
            var version = hasVersion
                ? javaReader.Read<sbyte>()
                : 1;

            var track = new LavaTrack(
                hash,
                title: javaReader.ReadString(),
                author: javaReader.ReadString(),
                duration: javaReader.Read<long>(),
                id: javaReader.ReadString(),
                isStream: javaReader.Read<bool>(),
                url: javaReader.Read<bool>()
                    ? javaReader.ReadString()
                    : string.Empty,
                position: default,
                canSeek: true);

            return track;
        }
    }
}