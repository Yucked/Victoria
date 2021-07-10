namespace Victoria.Payloads {
    internal struct VoiceState {
        /// <summary>
        /// 
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SessionId { get; set; }
    }
}