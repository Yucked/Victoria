using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Socks;
using Victoria.Common;
using Victoria.Common.Enums;
using Victoria.Common.Interfaces;
using Victoria.Lavalink.Payloads;

namespace Victoria.Lavalink
{
    /// <summary>
    /// </summary>
    public class LavaPlayer : IPlayer<LavaTrack>
    {
        private readonly ClientSock _sock;

        internal LavaPlayer(ClientSock sock, IVoiceChannel voiceChannel, ITextChannel textChannel)
        {
            _sock = sock;
            VoiceChannel = voiceChannel;
            TextChannel = textChannel;
        }

        /// <inheritdoc />
        public IVoiceState VoiceState { get; internal set; }

        /// <inheritdoc />
        public int Volume { get; private set; }

        /// <inheritdoc />
        public LavaTrack Track { get; internal set; }

        /// <inheritdoc />
        public PlayerState PlayerState { get; internal set; }

        /// <inheritdoc />
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <inheritdoc />
        public DefaultQueue<LavaTrack> Queue { get; private set; }

        /// <inheritdoc />
        public IVoiceChannel VoiceChannel { get; private set; }

        /// <inheritdoc />
        public ITextChannel TextChannel { get; }

        /// <inheritdoc />
        public async Task PlayAsync(LavaTrack track)
        {
            Ensure.NotNull(track);

            var payload = new PlayPayload(VoiceChannel.GuildId, track, false);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);

            Track = track;
            PlayerState = PlayerState.Playing;
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            Ensure.Constraints(PlayerState, PlayerState.Connected, PlayerState.Playing, PlayerState.Paused);

            var payload = new StopPayload(VoiceChannel.GuildId);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);

            PlayerState = PlayerState.Stopped;
        }

        /// <inheritdoc />
        public async Task PauseAsync()
        {
            Ensure.Constraints(PlayerState, PlayerState.Connected, PlayerState.Playing, PlayerState.Paused);

            var payload = new PausePayload(VoiceChannel.GuildId, true);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);

            PlayerState = PlayerState.Paused;
        }

        /// <inheritdoc />
        public async Task ResumeAsync()
        {
            Ensure.Constraints(PlayerState, PlayerState.Connected, PlayerState.Playing, PlayerState.Paused);

            var payload = new PausePayload(VoiceChannel.GuildId, false);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);

            PlayerState = Track is null
                ? PlayerState.Stopped
                : PlayerState.Playing;
        }

        /// <inheritdoc />
        public async Task<LavaTrack> SkipAsync(TimeSpan? delay = default)
        {
            Ensure.Constraints(PlayerState, PlayerState.Connected, PlayerState.Playing, PlayerState.Paused);

            if (!Queue.TryDequeue(out var track))
                Throw.Exception($"There are no items in {nameof(Queue)}.");

            await await Task.Delay(delay ?? TimeSpan.Zero)
                .ContinueWith(_ => PlayAsync(track))
                .ConfigureAwait(false);

            return track;
        }

        /// <inheritdoc />
        public async Task SeekAsync(TimeSpan position)
        {
            Ensure.NotNull(position);
            Ensure.Constraints(PlayerState, PlayerState.Connected, PlayerState.Playing, PlayerState.Paused);

            if (position.TotalMilliseconds > Track.Duration.TotalMilliseconds)
                Throw.OutOfRange(nameof(position),
                    $"Value must be no bigger than {Track.Duration.TotalMilliseconds}ms.");

            var payload = new SeekPayload(VoiceChannel.GuildId, position);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task UpdateVolumeAsync(ushort volume)
        {
            Volume = volume;
            var payload = new VolumePayload(VoiceChannel.GuildId, volume);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await StopAsync()
                .ConfigureAwait(false);

            var payload = new DestroyPayload(VoiceChannel.GuildId);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);

            GC.SuppressFinalize(this);

            Queue.Clear();
            Queue = default;
            Track = null;
            VoiceChannel = null;
            PlayerState = PlayerState.Disconnected;
        }

        /// <summary>
        ///     Plays the specified track with a custom start and end time.
        /// </summary>
        /// <param name="track">An instance of <see cref="LavaTrack" />.</param>
        /// <param name="startTime">Custom start time for track. Must be greater than 0.</param>
        /// <param name="endTime">Custom end time for track. Must be less than <see cref="LavaTrack.Duration" />.</param>
        /// <param name="noReplace">If true, this operation will be ignored if a track is already playing or paused.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws when start or end time are out of range.</exception>
        /// <exception cref="InvalidOperationException">Throws when star time is bigger than end time.</exception>
        public async Task PlayAsync(LavaTrack track, TimeSpan startTime, TimeSpan endTime, bool noReplace = false)
        {
            Ensure.NotNull(track, startTime, endTime);

            if (startTime.TotalMilliseconds < 0)
                Throw.OutOfRange(nameof(startTime), "Value must be greater than 0.");

            if (endTime.TotalMilliseconds < 0)
                Throw.OutOfRange(nameof(endTime), "Value must be greater than 0.");

            if (startTime <= endTime)
                Throw.InvalidOperation($"{nameof(endTime)} must be greather than {nameof(startTime)}.");

            var payload = new PlayPayload(VoiceChannel.GuildId, track.Hash, startTime, endTime, noReplace);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);

            Track = track;
            PlayerState = PlayerState.Playing;
        }

        /// <summary>
        ///     Change the <see cref="LavaPlayer" />'s equalizer. There are 15 bands (0-14) that can be changed.
        /// </summary>
        /// <param name="bands">
        ///     <see cref="Band" />
        /// </param>
        public async Task EqualizerAsync(IEnumerable<Band> bands)
        {
            Ensure.NotNull(bands);
            Ensure.Constraints(PlayerState, PlayerState.Connected, PlayerState.Playing, PlayerState.Paused);

            var payload = new EqualizerPayload(VoiceChannel.GuildId, bands);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Change the <see cref="LavaPlayer" />'s equalizer. There are 15 bands (0-14) that can be changed.
        /// </summary>
        /// <param name="bands">
        ///     <see cref="Band" />
        /// </param>
        public async Task EqualizerAsync(params Band[] bands)
        {
            Ensure.NotNull(bands);
            Ensure.Constraints(PlayerState, PlayerState.Connected, PlayerState.Playing, PlayerState.Paused);

            var payload = new EqualizerPayload(VoiceChannel.GuildId, bands);
            await _sock.SendAsync(payload)
                .ConfigureAwait(false);
        }

        internal void UpdatePlayer(Action<LavaPlayer> action)
        {
            action.Invoke(this);
        }
    }
}
