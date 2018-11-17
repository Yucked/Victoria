using System;
using System.IO;
using Victoria.Entities;

namespace Victoria.Utilities
{
    /// <summary>
    /// https://raw.githubusercontent.com/DSharpPlus/DSharpPlus/master/DSharpPlus.Lavalink/LavalinkUtil.cs
    /// </summary>
    public static class TrackResolver
    {
        public static LavaTrack DecodeTrack(string track)
        {
            const int TRACK_INFO_VERSIONED = 1;
            var raw = Convert.FromBase64String(track);

            var decoded = new LavaTrack
            {
                TrackString = track
            };

            using (var ms = new MemoryStream(raw))
            using (var br = new JavaBinaryReader(ms))
            {
                var messageHeader = br.ReadInt32();
                var messageFlags = (int) ((messageHeader & 0xC0000000L) >> 30);
                var version = (messageFlags & TRACK_INFO_VERSIONED) != 0 ? br.ReadSByte() & 0xFF : 1;

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