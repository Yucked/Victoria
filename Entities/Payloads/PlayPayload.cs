using System;
using System.Text.Json.Serialization;

namespace Victoria.Entities.Payloads
{
    internal sealed class PlayPayload : LavaPayload
    {
        [JsonPropertyName("track")]
        public LavaTrack Track { get; }

        [JsonPropertyName("startTime")]
        public int StartTime { get; }

        [JsonPropertyName("endTime")]
        public int EndTime { get; }

        [JsonPropertyName("noReplace")]
        public bool NoReplace { get; }

        public PlayPayload(ulong guildId, LavaTrack track,
                              TimeSpan start, TimeSpan end,
                              bool noReplace) : base(guildId, "play")
        {
            Track = track;
            StartTime = (int)start.TotalMilliseconds;
            EndTime = (int)end.TotalMilliseconds;
            NoReplace = noReplace;
        }
    }
}