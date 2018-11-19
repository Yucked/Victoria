using System;
using System.IO;
using Victoria.Entities;

namespace Victoria.Utilities
{
    /// <summary>
    /// https://raw.githubusercontent.com/DSharpPlus/DSharpPlus/master/DSharpPlus.Lavalink/Lavalinkcs
    /// </summary>
    public static class TrackResolver
    {
        public static LavaTrack DecodeTrack(string trackString)
        {
            const int trackInfoVersioned = 1;
            var raw = Convert.FromBase64String(trackString);

            var decoded = new LavaTrack
            {
                TrackString = trackString
            };

            using (var ms = new MemoryStream(raw))
            using (var br = new JavaBinaryReader(ms))
            {
                var messageHeader = br.ReadInt32();
                var messageFlags = (int) ((messageHeader & 0xC0000000L) >> 30);
                var version = (messageFlags & trackInfoVersioned) != 0 ? br.ReadSByte() & 0xFF : 1;

                decoded.Title = br.ReadJavaUtf8();
                decoded.Author = br.ReadJavaUtf8();
                decoded.length = br.ReadInt64();
                decoded.Id = br.ReadJavaUtf8();
                decoded.IsStream = br.ReadBoolean();

                var uri = br.ReadNullableString();
                decoded.Uri = uri != null && version >= 2 ? new Uri(uri) : null;
            }

            return decoded;
        }
    }
}