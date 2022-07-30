using System;
using System.Linq;
using System.Text;

namespace Victoria.Encoder {
    public readonly struct TrackEncoder {
        // Stuff required for the encoding
        private const int TRACK_VERSIONED = 1;
        private const int TRACK_VERSION = 2;

        // Takes in a Track and sets its hash property in place
        public static void Encode(LavaTrack track) {
            Span<byte> bytes = stackalloc byte[GetByteCount(track)];

            var javaWriter = new JavaBinaryWriter(bytes);
            javaWriter.Write<byte>(TRACK_VERSION);
            javaWriter.Write(track.Title);
            javaWriter.Write(track.Author);
            javaWriter.Write((long)track.Duration.TotalMilliseconds);
            javaWriter.Write(track.Id);
            javaWriter.Write(track.IsStream);
            javaWriter.WriteNullableText(track.Url); // Extension method
            javaWriter.Write(track.Source);
            javaWriter.Write((long)track.Position.TotalMilliseconds);
            javaWriter.WriteVersioned(TRACK_VERSIONED); // Extension method

            track.Hash = Convert.ToBase64String(bytes);
        }

        // Calculate the number of bytes needed to encode the track
        // in part to make sure we allocate just the right amount of bytes
        private static int GetByteCount(LavaTrack track) {
            // The value 23 was derived from:
            // 8 (a long for position) + 8 (a long for duration) + 4 (an int representing the "versioned")
            // + 1 (a byte for the version) + 1 (a bool indicating if it's a stream) + 1 (a bool that will be written before the url)
            // = 23 bytes.
            //
            // After that we need to calculate the number of bytes needed for the string properties (except of course the "Hash" property)
            // plus 2 for each of them, since every string has its length prepended to it encoded as a short (2 bytes)
            return 23 + typeof(LavaTrack).GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.Name != "Hash")
                .Sum(p => 2 + Encoding.UTF8.GetByteCount(p.GetValue(track).ToString()));
        }
    }
}
