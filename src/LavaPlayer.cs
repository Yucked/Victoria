using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Victoria.Enums;
using Victoria.Filters;
using Victoria.Payloads.Player;

namespace Victoria {
    /// <summary>
    /// Arguments for <see cref="o:LavaPlayer.PlayAsync"/>
    /// </summary>
    public sealed class PlayArgs {
        /// <summary>
        /// Which track to play, <see cref="LavaTrack"/>
        /// </summary>
        public LavaTrack Track { get; set; }

        /// <summary>
        /// Whether to replace the track. Returns <see cref="TrackEndReason.Replaced"/> when used.
        /// </summary>
        public bool NoReplace { get; set; }

        /// <summary>
        /// Set the volume of the player when playing <see cref="Track"/>.
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// Whether to pause the player when <see cref="Track"/> is ready to play.
        /// </summary>
        public bool ShouldPause { get; set; }

        /// <summary>
        /// Start time of <see cref="Track"/>.
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// End time of <see cref="Track"/>.
        /// </summary>
        public TimeSpan? EndTime { get; set; }
    }

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
        ///     Player's current volume.
        /// </summary>
        public int Volume { get; private set; }

        /// <summary>
        /// Whether or not the player is conencted to voice gateway.
        /// </summary>
        public bool IsConnected { get; internal set; }

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

        /// <summary>
        /// Plays the specified track with provided arguments.
        /// </summary>
        /// <param name="playArgsAction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="playArgsAction"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws when the volume is out of range.</exception>
        public async Task PlayAsync(Action<PlayArgs> playArgsAction) {
            var playArgs = new PlayArgs();
            playArgsAction.Invoke(playArgs);

            Track = playArgs.Track ?? throw new NullReferenceException(nameof(playArgs));
            PlayerState = playArgs.ShouldPause ? PlayerState.Paused : PlayerState.Playing;

            if (playArgs.Volume < 0) {
                throw new ArgumentOutOfRangeException(nameof(playArgs.Volume),
                    "Volume must be greater than or equal to 0.");
            }

            if (playArgs.Volume > 1000) {
                throw new ArgumentOutOfRangeException(nameof(playArgs.Volume),
                    "Volume must be less than or equal to 1000.");
            }

            Volume = playArgs.Volume;
            await _lavaSocket.SendAsync(new PlayPayload(VoiceChannel.GuildId, playArgs));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lavaTrack"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task PlayAsync(LavaTrack lavaTrack) {
            Track = lavaTrack ?? throw new NullReferenceException(nameof(lavaTrack));
            PlayerState = PlayerState.Playing;
            await _lavaSocket.SendAsync(new PlayPayload(VoiceChannel.GuildId, new PlayArgs {
                    Track = lavaTrack,
                    Volume = 100,
                    ShouldPause = false
                }))
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
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
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
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
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
        public async Task<(LavaTrack Skipped, LavaTrack Current)> SkipAsync(TimeSpan? delay = default) {
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
            }

            if (!Queue.TryDequeue(out var lavaTrack)) {
                throw new InvalidOperationException("Can't skip to the next item in queue.");
            }

            if (lavaTrack.GetType() != typeof(LavaTrack)) {
                throw new InvalidCastException($"Couldn't cast {lavaTrack.GetType()} to {typeof(LavaTrack)}.");
            }

            var skippedTrack = Track;
            await Task.Delay(delay ?? TimeSpan.Zero);
            await PlayAsync(lavaTrack);

            return (skippedTrack, lavaTrack);
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

            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
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
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
            }

            foreach (var band in bands) {
                _equalizer[band.Band] = band;
            }

            var payload = new EqualizerPayload(VoiceChannel.GuildId, bands);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Apply a single filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="volume"></param>
        /// <param name="equalizerBands"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task ApplyFilterAsync(IFilter filter, double volume = 1.0, params EqualizerBand[] equalizerBands) {
            if (filter == null) {
                throw new ArgumentNullException(nameof(filter));
            }

            Volume = (int) volume * 100;
            return _lavaSocket.SendAsync(new FilterPayload(VoiceChannel.GuildId, filter, volume, equalizerBands));
        }

        /// <summary>
        /// Apply multiple filters
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="volume"></param>
        /// <param name="equalizerBands"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task ApplyFiltersAsync(IEnumerable<IFilter> filters, double volume = 1.0,
                                      params EqualizerBand[] equalizerBands) {
            if (filters == null) {
                throw new ArgumentNullException(nameof(filters));
            }

            if (equalizerBands != null) {
                foreach (var band in equalizerBands) {
                    _equalizer[band.Band] = band;
                }
            }

            Volume = (int) volume * 100;
            return _lavaSocket.SendAsync(new FilterPayload(VoiceChannel.GuildId, filters, volume, equalizerBands));
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            await StopAsync()
                .ConfigureAwait(false);

            var payload = new DestroyPayload(VoiceChannel.GuildId);
            await _lavaSocket.SendAsync(payload)
                .ConfigureAwait(false);

            Queue.Clear();
            Queue = default;
            Track = null;
            VoiceChannel = null;
            PlayerState = PlayerState.None;
        }
    }
}