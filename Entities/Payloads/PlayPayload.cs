using System;
using Newtonsoft.Json;
using Victoria.Entities.Responses.LoadTracks;

namespace Victoria.Entities.Payloads
{
    internal sealed class PlayPayload : LavaPayload
    {
        [JsonProperty("track")]
        public Track Track { get; }

        [JsonProperty("startTime")]
        public int StartTime { get; }

        [JsonProperty("endTime")]
        public int EndTime { get; }

        [JsonProperty("noReplace")]
        public bool NoReplace { get; }

        public PlayPayload(ulong guildId, Track track,
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