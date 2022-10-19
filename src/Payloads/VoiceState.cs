namespace Victoria.Payloads {
    internal record VoiceState {
        /// <summary>
        /// 
        /// </summary>
        public ulong UserId { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public ulong ChannelId { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public ulong GuildId { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public string SessionId { get; init; }
    }
}