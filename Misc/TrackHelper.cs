using System;
using System.IO;
using Victoria.Objects;

// Taken From: https://github.com/DSharpPlus/DSharpPlus/blob/master/DSharpPlus.Lavalink/LavalinkUtil.cs
// Coz I'm not smart enough for this. Thanks Emzi.
namespace Victoria.Misc
{
    public sealed class TrackHelper
    {
        public static LavaTrack DecodeTrack(string track)
        {
            // https://github.com/sedmelluq/lavaplayer/blob/804cd1038229230052d9b1dee5e6d1741e30e284/main/src/main/java/com/sedmelluq/discord/lavaplayer/player/DefaultAudioPlayerManager.java#L63-L64
            const int TRACK_INFO_VERSIONED = 1;
            //const int TRACK_INFO_VERSION = 2;

            var raw = Convert.FromBase64String(track);

            var decoded = new LavaTrack
            {
                TrackString = track
            };

            using (var ms = new MemoryStream(raw))
            using (var br = new BinaryHelper(ms))
            {
                // https://github.com/sedmelluq/lavaplayer/blob/b0c536098c4f92e6d03b00f19221021f8f50b19b/main/src/main/java/com/sedmelluq/discord/lavaplayer/tools/io/MessageInput.java#L37-L39
                var messageHeader = br.ReadInt32();
                var messageFlags = (int) ((messageHeader & 0xC0000000L) >> 30);
                //if (messageSize != raw.Length)
                //    Warn($"Size conflict: {messageSize} but was {raw.Length}");

                // https://github.com/sedmelluq/lavaplayer/blob/804cd1038229230052d9b1dee5e6d1741e30e284/main/src/main/java/com/sedmelluq/discord/lavaplayer/player/DefaultAudioPlayerManager.java#L268

                // java bytes are signed
                // https://docs.oracle.com/javase/7/docs/api/java/io/DataInput.html#readByte()
                var version = (messageFlags & TRACK_INFO_VERSIONED) != 0 ? br.ReadSByte() & 0xFF : 1;
                //if (version != TRACK_INFO_VERSION)
                //    Warn($"Version conflict: Expected {TRACK_INFO_VERSION} but got {version}");

                decoded.Title = br.ReadJavaUtf8();

                decoded.Author = br.ReadJavaUtf8();

                decoded.length = br.ReadInt64();

                decoded.Id = br.ReadJavaUtf8();

                decoded.IsStream = br.ReadBoolean();

                var uri = br.ReadNullableString();
                if (uri != null && version >= 2)
                    decoded.Uri = new Uri(uri);
                else
                    decoded.Uri = null;
            }

            return decoded;
        }
    }
}