using System;
using System.IO;
using Victoria.Entities;

namespace Victoria.Helpers
{
    internal sealed class TrackHelper
    {
        public static LavaTrack DecodeTrack(string trackString)
        {
            const int trackInfoVersioned = 1;
            var raw = Convert.FromBase64String(trackString);

            var decoded = new LavaTrack
            {
                Hash = trackString
            };

            using var ms = new MemoryStream(raw);
            using var jb = new JavaBinaryHelper(ms);

            var messageHeader = jb.ReadInt32();
            var messageFlags = (int)((messageHeader & 0xC0000000L) >> 30);
            var version = (messageFlags & trackInfoVersioned) != 0 ? jb.ReadSByte() & 0xFF : 1;

            decoded.Title = jb.ReadJavaUtf8();
            decoded.Author = jb.ReadJavaUtf8();
            decoded.TrackLength = jb.ReadInt64();
            decoded.Id = jb.ReadJavaUtf8();
            decoded.IsStream = jb.ReadBoolean();

            var uri = jb.ReadNullableString();
            decoded.Uri = uri != null && version >= 2 ? new Uri(uri) : null;

            return decoded;
        }
    }
}