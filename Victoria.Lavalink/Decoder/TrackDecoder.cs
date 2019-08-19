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
            var header = javaReader.Read<int>();
            var flags = (int) ((header & 0xC0000000L) >> 30);
            var hasVersion = (flags & 1) != 0;
            var version = hasVersion
                ? javaReader.Read<sbyte>()
                : 1;

            // Get track information
            var track = new LavaTrack()
                .WithHash(hash)
                .WithTitle(javaReader.ReadString())
                .WithAuthor(javaReader.ReadString())
                .WithDuration(javaReader.Read<long>())
                .WithId(javaReader.ReadString())
                .WithStream(javaReader.Read<bool>())
                .WithUrl(javaReader.Read<bool>()
                    ? javaReader.ReadString()
                    : string.Empty);

            return track;
        }
    }
}
