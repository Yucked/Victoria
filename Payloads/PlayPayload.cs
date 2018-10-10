using System;
using Newtonsoft.Json;
using Victoria.Objects;

namespace Victoria.Payloads
{
    internal sealed class PlayPayload : LavaPayload
    {
        public PlayPayload(ulong guildId, LavaTrack track)
            : base("play", guildId)
        {
            Track = track.TrackString;
        }

        [JsonProperty("track")] 
        public string Track { get; }
    }

    internal sealed class PlayPartialPayload : LavaPayload
    {
        public PlayPartialPayload(ulong id, LavaTrack track, TimeSpan start, TimeSpan stop) : base("play", id)
        {
            Track = track.TrackString;
            StopTime = (long) stop.TotalMilliseconds;
            StartTime = (long) start.TotalMilliseconds;
        }

        [JsonProperty("track")] 
        public string Track { get; }

        [JsonProperty("startTime")] 
        public long StartTime { get; }

        [JsonProperty("stopTime")] 
        public long StopTime { get; }
    }
}