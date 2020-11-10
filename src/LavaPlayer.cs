using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Victoria.Enums;
using Victoria.Payloads;

namespace Victoria {
    /// <summary>
    /// Represents a <see cref="IVoiceChannel"/> connection.
    /// </summary>
    public class LavaPlayer : IAsyncDisposable {
        /// <summary>
        /// </summary>
        public IReadOnlyCollection<EqualizerBand> Equalizer
            => _equalizer.Values as IReadOnlyCollection<EqualizerBand>;

        /// <summary>
        ///     Last time player was updated.
        /// </summary>
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <summary>
        ///     Player's current state.
        /// </summary>
        public PlayerState PlayerState { get; internal set; }

        /// <summary>
        ///     Default queue.
        /// </summary>
        public DefaultQueue<LavaTrack> Queue { get; private set; }

        /// <summary>
        ///     Channel bound to this player.
        /// </summary>
        public ITextChannel TextChannel { get; internal set; }

        /// <summary>
        ///     Current track that is playing.
        /// </summary>
        public LavaTrack Track { get; internal set; }

        /// <summary>
        ///     Voice channel this player is connected to.
        /// </summary>
        public IVoiceChannel VoiceChannel { get; internal set; }

        /// <summary>
        ///     Voice server this player is connected to.
        /// </summary>
        public SocketVoiceServer VoiceServer { get; internal set; }

        /// <summary>
        ///     Player's current voice state.
        /// </summary>
        public IVoiceState VoiceState { get; internal set; }

        /// <summary>
        ///     Player's current volume.
        /// </summary>
        public int Volume { get; private set; }

        private readonly IDictionary<int, EqualizerBand> _equalizer;

        private readonly LavaSocket _lavaSocket;

        /// <summary>
        ///     Represents a <see cref="IGuild" /> voice connection.
        /// </summary>
        /// <param name="lavaSocket">
        ///     <see cref="LavaSocket" />
        /// </param>
        /// <param name="voiceChannel">Voice channel to connect to.</param>
        /// <param name="textChannel">Text channel this player is bound to.</param>
        public LavaPlayer(LavaSocket lavaSocket, IVoiceChannel voiceChannel, ITextChannel textChannel) {
            _lavaSocket = lavaSocket;
            VoiceChannel = voiceChannel;
            TextChannel = textChannel;
            Queue = new DefaultQueue<LavaTrack>();
            _equalizer = new Dictionary<int, EqualizerBand>(15);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            await StopAsync()
                .ConfigureAwait(false);

            var payload = new DestroyPayload(VoiceChannel.GuildId);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);

            GC.SuppressFinalize(this);

            Queue.Clear();
            Queue = default;
            Track = null;
            VoiceChannel = null;
            PlayerState = PlayerState.Disconnected;
        }

        /// <summary>
        ///     Plays the specified track.
        /// </summary>
        /// <param name="track">An instance of <see cref="LavaTrack" />.</param>
        public async Task PlayAsync(LavaTrack track) {
            if (track == null) {
                throw new ArgumentNullException(nameof(track));
            }

            var payload = new PlayPayload(VoiceChannel.GuildId, track, false);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);

            Track = track;
            PlayerState = PlayerState.Playing;
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
        public async Task PlayAsync(LavaTrack track, TimeSpan startTime, TimeSpan endTime, bool noReplace = false) {
            if (startTime.TotalMilliseconds < 0) {
                throw new ArgumentOutOfRangeException(nameof(startTime), "Value must be greater than 0.");
            }

            if (endTime.TotalMilliseconds < 0) {
                throw new ArgumentOutOfRangeException(nameof(endTime), "Value must be greater than 0.");
            }

            if (endTime <= startTime) {
                throw new InvalidOperationException($"{nameof(endTime)} must be greather than {nameof(startTime)}.");
            }

            Track = track ?? throw new ArgumentNullException(nameof(track));
            PlayerState = PlayerState.Playing;

            var payload = new PlayPayload(VoiceChannel.GuildId, track.Hash, startTime, endTime, noReplace);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Stops the current track if any is playing.
        /// </summary>
        public async Task StopAsync() {
            PlayerState = PlayerState.Stopped;
            var payload = new StopPayload(VoiceChannel.GuildId);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Pauses the current track if any is playing.
        /// </summary>
        public async Task PauseAsync() {
            if (!PlayerState.EnsureState()) {
                throw new InvalidOperationException(
                    "Player state doesn't match any of the following states: Connected, Playing, Paused.");
            }

            PlayerState = Track is null
                ? PlayerState.Stopped
                : PlayerState.Paused;

            var payload = new PausePayload(VoiceChannel.GuildId, true);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Resumes the current track if any is playing.
        /// </summary>
        public async Task ResumeAsync() {
            if (!PlayerState.EnsureState()) {
                throw new InvalidOperationException(
                    "Player state doesn't match any of the following states: Connected, Playing, Paused.");
            }

            PlayerState = Track is null
                ? PlayerState.Stopped
                : PlayerState.Playing;

            var payload = new PausePayload(VoiceChannel.GuildId, false);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Skips the current track after the specified delay.
        /// </summary>
        /// <param name="delay">If set to null, skips instantly otherwise after the specified value.</param>
        /// <returns>
        ///     The next <see cref="LavaTrack" />.
        /// </returns>
        public async Task<LavaTrack> SkipAsync(TimeSpan? delay = default) {
            if (!PlayerState.EnsureState()) {
                throw new InvalidOperationException(
                    "Player state doesn't match any of the following states: Connected, Playing, Paused.");
            }

            if (!Queue.TryDequeue(out var queueable)) {
                throw new InvalidOperationException("Can't skip to the next item in queue.");
            }

            if (!(queueable is LavaTrack track)) {
                throw new InvalidCastException($"Couldn't cast {queueable.GetType()} to {typeof(LavaTrack)}.");
            }

            await await Task.Delay(delay ?? TimeSpan.Zero)
                .ContinueWith(_ => PlayAsync(track))
                .ConfigureAwait(false);

            return track;
        }

        /// <summary>
        ///     Seeks the current track to specified position.
        /// </summary>
        /// <param name="position">Position must be less than <see cref="LavaTrack.Duration" />.</param>
        /// <returns></returns>
        public async Task SeekAsync(TimeSpan? position) {
            if (position == null) {
                throw new ArgumentNullException(nameof(position));
            }

            if (!PlayerState.EnsureState()) {
                throw new InvalidOperationException(
                    "Player state doesn't match any of the following states: Connected, Playing, Paused.");
            }

            if (position.Value.TotalMilliseconds > Track.Duration.TotalMilliseconds) {
                throw new ArgumentOutOfRangeException(nameof(position),
                    $"Value must be no bigger than {Track.Duration.TotalMilliseconds}ms.");
            }

            var payload = new SeekPayload(VoiceChannel.GuildId, position.Value);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Changes the current volume and updates <see cref="Volume" />.
        /// </summary>
        public async Task UpdateVolumeAsync(ushort volume) {
            Volume = volume;
            var payload = new VolumePayload(VoiceChannel.GuildId, volume);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     Change the <see cref="LavaPlayer" />'s equalizer. There are 15 bands (0-14) that can be changed.
        /// </summary>
        /// <param name="bands">
        ///     <see cref="EqualizerBand" />
        /// </param>
        public async Task EqualizerAsync(params EqualizerBand[] bands) {
            if (!PlayerState.EnsureState()) {
                throw new InvalidOperationException(
                    "Player state doesn't match any of the following states: Connected, Playing, Paused.");
            }

            foreach (var band in bands) {
                _equalizer[band.Band] = band;
            }

            var payload = new EqualizerPayload(VoiceChannel.GuildId, bands);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);
        }
    }
}