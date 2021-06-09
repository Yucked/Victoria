using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Victoria.Payloads.Player;
using Victoria.Player.Args;
using Victoria.WebSocket;

namespace Victoria.Player {
    /// <summary>
    /// Represents a <see cref="IVoiceChannel"/> connection.
    /// </summary>
    public class LavaPlayer : IAsyncDisposable {
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
        public Vueue<LavaTrack> Vueue { get; }

        /// <summary>
        ///     Channel bound to this player.
        /// </summary>
        public ITextChannel TextChannel { get; private set; }

        /// <summary>
        ///     Current track that is playing.
        /// </summary>
        public LavaTrack Track { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public ulong VoiceChannelId { get; }

        /// <summary>
        ///     Voice server this player is connected to.
        /// </summary>
        public SocketVoiceServer VoiceServer { get; internal set; }

        /// <summary>
        ///     Player's current voice state.
        /// </summary>
        public IVoiceState VoiceState { get; internal set; }

        private readonly IDictionary<int, double> _bands;
        private readonly WebSocketClient _socketClient;

        /// <summary>
        /// Represents a VoiceChannel connection.
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="voiceChannelId"></param>
        /// <param name="textChannel"></param>
        public LavaPlayer(WebSocketClient socketClient, ulong voiceChannelId, ITextChannel textChannel) {
            _socketClient = socketClient;
            VoiceChannelId = voiceChannelId;
            TextChannel = textChannel;
            Vueue = new Vueue<LavaTrack>();
            _bands = new Dictionary<int, double>(15);
        }

        /// <summary>
        /// Plays the specified track with provided arguments.
        /// </summary>
        /// <param name="playArgs"><see cref="PlayArgs"/></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="playArgs"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws when the volume is out of range.</exception>
        public Task PlayAsync(PlayArgs playArgs) {
            Track = playArgs.Track ?? throw new ArgumentNullException(nameof(playArgs));
            PlayerState = playArgs.ShouldPause ? PlayerState.Paused : PlayerState.Playing;

            return (Volume = playArgs.Volume) switch {
                < 0 => throw new ArgumentOutOfRangeException(nameof(playArgs.Volume),
                    "Volume must be greater than or equal to 0."),
                > 1000 => throw new ArgumentOutOfRangeException(nameof(playArgs.Volume),
                    "Volume must be less than or equal to 1000."),
                _ => _socketClient.SendAsync(new PlayPayload(VoiceChannelId, playArgs))
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when <see cref="PlayerState"/> is invalid.</exception>
        public Task StopAsync() {
            PlayerState = PlayerState.Stopped;
            return _socketClient.SendAsync(new StopPayload(VoiceChannelId));
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

            return _socketClient.SendAsync(new PausePayload(VoiceChannelId, true));
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

            return _socketClient.SendAsync(new PausePayload(VoiceChannelId, false));
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
            await await Task.Delay(skipAfter ?? TimeSpan.Zero)
                .ContinueWith(_ => PlayAsync(new PlayArgs {
                    Track = lavaTrack,
                    NoReplace = false
                }));

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
                throw new ArgumentOutOfRangeException(nameof(seekPosition), "");
            }

            return _socketClient.SendAsync(new SeekPayload(VoiceChannelId, seekPosition));
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
                _ => _socketClient.SendAsync(new VolumePayload(VoiceChannelId, Volume = volume))
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

            return _socketClient.SendAsync(new EqualizerPayload(VoiceChannelId, equalizerBands));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textChannel"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetTextChannel(ITextChannel textChannel) {
            TextChannel = textChannel ?? throw new ArgumentNullException(nameof(textChannel));
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            await StopAsync()
                .ConfigureAwait(false);
            await _socketClient.SendAsync(new DestroyPayload(VoiceChannelId));

            Vueue.Clear();
            Track = null;
            PlayerState = PlayerState.None;
        }
    }
}