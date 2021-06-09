using System;
using System.Threading.Tasks;

namespace Victoria.Wrappers {
    /// <summary>
    /// 
    /// </summary>
    public record DiscordClient {
        /// <summary>
        /// 
        /// </summary>
        public int Shards { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public ulong UserId { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public Func<VoiceServer, ValueTask> OnVoiceServerUpdated;

        /// <summary>
        /// 
        /// </summary>
        public Func<VoiceState, ValueTask> OnUserVoiceStateUpdated;
    }
}