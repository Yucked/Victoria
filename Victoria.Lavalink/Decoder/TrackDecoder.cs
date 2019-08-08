using System;
using System.IO;

namespace Victoria.Lavalink.Decoder
{
    /// <summary>
    /// </summary>
    public readonly struct TrackDecoder
    {
        /// <summary>
        ///     Decodes the hash for the specified track.
        /// </summary>
        /// <param name="hash">Track's hash.</param>
        /// <returns>
        ///     <see cref="LavaTrack" />
        /// </returns>
        public static LavaTrack Decode(string hash)
        {
            var bytes = Convert.FromBase64String(hash);

            using var memStream = new MemoryStream(bytes);
            using var javaReader = new JavaBinaryReader(memStream);

            // Reading header
            var header = javaReader.ReadInt32();
            var flags = (int) ((header & 0xC0000000L) >> 30);
            var hasVersion = (flags & 1) != 0;
            var version = hasVersion
                ? javaReader.ReadSByte()
                : 1;

            // Get track information
            var track = new LavaTrack()
                .WithHash(hash)
                .WithTitle(javaReader.ReadString())
                .WithAuthor(javaReader.ReadString())
                .WithDuration(javaReader.ReadInt64())
                .WithId(javaReader.ReadString())
                .WithStream(javaReader.ReadBoolean())
                .WithUrl(javaReader.ReadBoolean()
                    ? javaReader.ReadString()
                    : string.Empty);

            return track;
        }
    }
}