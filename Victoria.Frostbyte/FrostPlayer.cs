using System;
using System.Threading.Tasks;
using Discord;
using Socks;
using Victoria.Common;
using Victoria.Common.Enums;
using Victoria.Common.Interfaces;
using Victoria.Frostbyte.Infos;

namespace Victoria.Frostbyte
{
    /// <inheritdoc />
    public class FrostPlayer : IPlayer<TrackInfo>
    {
        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
        }

        /// <inheritdoc />
        public IVoiceState VoiceState { get; internal set; }

        /// <inheritdoc />
        public int Volume { get; private set; }

        /// <inheritdoc />
        public TrackInfo Track { get; internal set; }

        /// <inheritdoc />
        public PlayerState PlayerState { get; private set; }

        /// <inheritdoc />
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <inheritdoc />
        public DefaultQueue<TrackInfo> Queue { get; }

        /// <inheritdoc />
        public IVoiceChannel VoiceChannel { get; internal set; }

        /// <inheritdoc />
        public ITextChannel TextChannel { get; internal set; }

        private readonly ClientSock _sock;

        internal FrostPlayer(ClientSock clientSock, IVoiceChannel voiceChannel, ITextChannel textChannel)
        {
            _sock = clientSock;
            VoiceChannel = voiceChannel;
            TextChannel = textChannel;
            Queue = new DefaultQueue<TrackInfo>();
        }

        /// <inheritdoc />
        public async Task PlayAsync(TrackInfo track)
        {
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
        }

        /// <inheritdoc />
        public async Task PauseAsync()
        {
        }

        /// <inheritdoc />
        public async Task ResumeAsync()
        {
        }

        /// <inheritdoc />
        public async Task<TrackInfo> SkipAsync(TimeSpan? delay = default)
        {
            return default;
        }

        /// <inheritdoc />
        public async Task SeekAsync(TimeSpan position)
        {
        }

        /// <inheritdoc />
        public async Task UpdateVolumeAsync(ushort volume)
        {
        }
    }
}
