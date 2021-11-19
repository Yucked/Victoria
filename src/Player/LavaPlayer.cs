using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Victoria.Payloads.Player;
using Victoria.Player.Args;
using Victoria.Player.Filters;
using Victoria.WebSocket;

namespace Victoria.Player {
    /// <summary>
    /// 
    /// </summary>
    public class LavaPlayer : LavaPlayer<LavaTrack> {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="voiceChannel"></param>
        /// <param name="textChannel"></param>
        public LavaPlayer(WebSocketClient socketClient, IVoiceChannel voiceChannel, ITextChannel textChannel)
            : base(socketClient, voiceChannel, textChannel) { }
    }

    /// <summary>
    /// Represents a <see cref="IVoiceChannel"/> connection.
    /// </summary>
    public class LavaPlayer<TLavaTrack> : IAsyncDisposable
        where TLavaTrack : LavaTrack {
        /// <summary>
        /// </summary>
        public IReadOnlyCollection<EqualizerBand> Bands
            => _bands.Values as IReadOnlyCollection<EqualizerBand>;

        /// <summary>
        ///     Player's current volume.
        /// </summary>
        public int Volume { get; private set; }

        /// <summary>
        ///     Last time player was updated.
        /// </summary>
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <summary>
        ///     Player's current state.
        /// </summary>
        public PlayerState PlayerState { get; internal set; }

        /// <summary>
        ///     Default Vueue.
        /// </summary>
        public Vueue<TLavaTrack> Vueue { get; }

        /// <summary>
        ///     Channel bound to this player.
        /// </summary>
        public ITextChannel TextChannel { get; private set; }

        /// <summary>
        ///     Current track that is playing.
        /// </summary>
        public TLavaTrack Track { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public IVoiceChannel VoiceChannel { get; }

        /// <summary>
        /// Is player connected to discord's websocket
        /// </summary>
        public bool IsConnected { get; internal set; }

        private readonly IDictionary<int, double> _bands;
        private readonly WebSocketClient _socketClient;
        private readonly ulong _guildId;

        /// <summary>
        /// Represents a VoiceChannel connection.
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="voiceChannel"></param>
        /// <param name="textChannel"></param>
        public LavaPlayer(WebSocketClient socketClient, IVoiceChannel voiceChannel, ITextChannel textChannel) {
            _socketClient = socketClient;
            VoiceChannel = voiceChannel;
            TextChannel = textChannel;
            Vueue = new Vueue<TLavaTrack>();
            _bands = new Dictionary<int, double>(15);
            _guildId = voiceChannel.GuildId;
        }

        /// <summary>
        /// Plays the specified track with provided arguments.
        /// </summary>
        /// <param name="playArgsAction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="playArgsAction"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws when the volume is out of range.</exception>
        public Task PlayAsync(Action<PlayArgs<TLavaTrack>> playArgsAction) {
            var playArgs = new PlayArgs<TLavaTrack>();
            playArgsAction.Invoke(playArgs);

            Track = playArgs.Track ?? throw new NullReferenceException(nameof(playArgs));
            PlayerState = playArgs.ShouldPause ? PlayerState.Paused : PlayerState.Playing;

            return (Volume = playArgs.Volume) switch {
                < 0 => throw new ArgumentOutOfRangeException(nameof(playArgs.Volume),
                    "Volume must be greater than or equal to 0."),
                > 1000 => throw new ArgumentOutOfRangeException(nameof(playArgs.Volume),
                    "Volume must be less than or equal to 1000."),
                _ => _socketClient.SendAsync(new PlayPayload<TLavaTrack>(_guildId, playArgs), false,
                    VictoriaExtensions.JsonOptions)
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lavaTrack"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Task PlayAsync(TLavaTrack lavaTrack) {
            Track = lavaTrack ?? throw new NullReferenceException(nameof(lavaTrack));
            PlayerState = PlayerState.Playing;
            return _socketClient.SendAsync(new PlayPayload<TLavaTrack>(_guildId, new PlayArgs<TLavaTrack> {
                Track = lavaTrack,
                Volume = 100,
                ShouldPause = false
            }), false, VictoriaExtensions.JsonOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when <see cref="PlayerState"/> is invalid.</exception>
        public Task StopAsync() {
            PlayerState = PlayerState.Stopped;
            return _socketClient.SendAsync(new StopPayload(_guildId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when <see cref="PlayerState"/> is invalid.</exception>
        public Task PauseAsync() {
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
            }

            PlayerState = Track is null
                ? PlayerState.Stopped
                : PlayerState.Paused;

            return _socketClient.SendAsync(new PausePayload(_guildId, true));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when <see cref="PlayerState"/> is invalid.</exception>
        public Task ResumeAsync() {
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
            }

            PlayerState = Track is null
                ? PlayerState.Stopped
                : PlayerState.Playing;

            return _socketClient.SendAsync(new PausePayload(_guildId, false));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skipAfter"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Throws when <see cref="PlayerState"/> is invalid.</exception>
        public async Task<(LavaTrack Skipped, LavaTrack Current)> SkipAsync(TimeSpan? skipAfter = default) {
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
            }

            if (!Vueue.TryDequeue(out var lavaTrack)) {
                throw new InvalidOperationException("There aren't any more tracks in the Vueue.");
            }

            var skippedTrack = Track;
            await Task.Delay(skipAfter ?? TimeSpan.Zero);
            await PlayAsync(lavaTrack);

            return (skippedTrack, lavaTrack);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seekPosition"></param>
        /// <exception cref="InvalidOperationException">Throws when <see cref="PlayerState"/> is invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="seekPosition"/> is greater than <see cref="Track"/> length.</exception>
        public Task SeekAsync(TimeSpan seekPosition) {
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
            }

            if (seekPosition.TotalMilliseconds > Track.Duration.TotalMilliseconds) {
                throw new ArgumentOutOfRangeException(nameof(seekPosition),
                    "Specified position is greater than track's total duration.");
            }

            return _socketClient.SendAsync(new SeekPayload(_guildId, seekPosition));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="volume"></param>
        /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="volume"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="volume"/> is greater than 1000.</exception>
        public Task SetVolumeAsync(int volume) {
            return volume switch {
                < 0 => throw new ArgumentOutOfRangeException(nameof(volume),
                    "Volume must be greater than or equal to 0."),
                > 1000 => throw new ArgumentOutOfRangeException(nameof(volume),
                    "Volume must be less than or equal to 1000."),
                _ => _socketClient.SendAsync(new VolumePayload(_guildId, Volume = volume))
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when <see cref="PlayerState"/> is invalid.</exception>
        public Task EqualizerAsync(params EqualizerBand[] equalizerBands) {
            foreach (var band in equalizerBands) {
                _bands[band.Band] = band.Gain;
            }

            return _socketClient.SendAsync(new EqualizerPayload(_guildId, equalizerBands));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textChannel"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetTextChannel(ITextChannel textChannel) {
            TextChannel = textChannel ?? throw new ArgumentNullException(nameof(textChannel));
        }

        /// <summary>
        /// 
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

            Volume = (int)volume * 100;
            return _socketClient.SendAsync(new FilterPayload(_guildId, filter, volume, equalizerBands));
        }

        /// <summary>
        /// 
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
                    _bands[band.Band] = band.Gain;
                }
            }

            Volume = (int)volume * 100;
            return _socketClient.SendAsync(new FilterPayload(_guildId, filters, volume, equalizerBands));
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            await StopAsync()
                .ConfigureAwait(false);
            await _socketClient.SendAsync(new DestroyPayload(_guildId));

            Vueue.Clear();
            Track = null;
            PlayerState = PlayerState.None;
            GC.SuppressFinalize(this);
        }
    }
}