using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria.Enums;
using Victoria.Interfaces;
using Victoria.Payloads.Player;
using Victoria.WebSocket;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Victoria {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TLavaTrack"></typeparam>
    public abstract class AbstractLavaPlayer<TLavaTrack> : ILavaPlayer<TLavaTrack> where TLavaTrack : ILavaTrack {
        /// <inheritdoc />
        public TLavaTrack Track { get; private set; }

        /// <inheritdoc />
        public int Volume { get; private set; }

        /// <inheritdoc />
        public DateTimeOffset LastUpdate { get; }

        /// <inheritdoc />
        public PlayerState PlayerState { get; private set; }

        /// <inheritdoc />
        public ulong VoiceChannelId { get; init; }

        /// <inheritdoc />
        public IReadOnlyCollection<EqualizerBand> Bands
            => _bands.Values as IReadOnlyCollection<EqualizerBand>;

        /// <inheritdoc />
        public LavaQueue<TLavaTrack> Queue { get; }

        private readonly IDictionary<int, double> _bands;
        private readonly WebSocketClient _webSocketClient;

        /// <summary>
        /// 
        /// </summary>
        protected AbstractLavaPlayer(WebSocketClient webSocketClient, ulong voiceChannelId) {
            _webSocketClient = webSocketClient;
            VoiceChannelId = voiceChannelId;
            Volume = 100;

            _bands = new Dictionary<int, double>(15);
            Queue = new LavaQueue<TLavaTrack>();
        }

        /// <inheritdoc />
        public ValueTask PlayAsync(TLavaTrack lavaTrack, bool noReplace = true, int volume = default,
                                   bool shouldPause = false) {
            Track = lavaTrack ?? throw new ArgumentNullException(nameof(lavaTrack));
            PlayerState = shouldPause ? PlayerState.Paused : PlayerState.Playing;

            return volume switch {
                < 0 => throw new ArgumentOutOfRangeException(nameof(volume),
                    "Volume must be greater than or equal to 0."),
                > 1000 => throw new ArgumentOutOfRangeException(nameof(volume),
                    "Volume must be less than or equal to 1000."),
                _ => _webSocketClient.SendAsync(
                    new PlayPayload(VoiceChannelId, lavaTrack.Hash, noReplace, Volume = volume, shouldPause))
            };
        }

        /// <inheritdoc />
        public ValueTask PlayAsync(TLavaTrack lavaTrack, TimeSpan startTime, TimeSpan stopTime,
                                   bool noReplace = false, int volume = default, bool shouldPause = false) {
            if (startTime.TotalMilliseconds < 0) {
                throw new ArgumentOutOfRangeException(nameof(startTime), "Value must be greater than 0.");
            }

            if (stopTime.TotalMilliseconds < 0) {
                throw new ArgumentOutOfRangeException(nameof(stopTime), "Value must be greater than 0.");
            }

            if (stopTime <= startTime) {
                throw new InvalidOperationException($"{nameof(stopTime)} must be greather than {nameof(startTime)}.");
            }

            Track = lavaTrack ?? throw new ArgumentNullException(nameof(lavaTrack));
            PlayerState = shouldPause ? PlayerState.Paused : PlayerState.Playing;

            return volume switch {
                < 0 => throw new ArgumentOutOfRangeException(nameof(volume),
                    "Volume must be greater than or equal to 0."),
                > 1000 => throw new ArgumentOutOfRangeException(nameof(volume),
                    "Volume must be less than or equal to 1000."),
                _ => _webSocketClient.SendAsync(
                    new PlayPayload(VoiceChannelId, lavaTrack.Hash, startTime, stopTime, noReplace,
                        Volume = volume, shouldPause))
            };
        }

        /// <inheritdoc />
        public ValueTask StopAsync() {
            PlayerState = PlayerState.Stopped;
            return _webSocketClient.SendAsync(new StopPayload(VoiceChannelId));
        }

        /// <inheritdoc />
        public ValueTask PauseAsync() {
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
            }

            PlayerState = Track is null
                ? PlayerState.Stopped
                : PlayerState.Paused;

            return _webSocketClient.SendAsync(new PausePayload(VoiceChannelId, true));
        }

        /// <inheritdoc />
        public ValueTask ResumeAsync() {
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
            }

            PlayerState = Track is null
                ? PlayerState.Stopped
                : PlayerState.Playing;

            return _webSocketClient.SendAsync(new PausePayload(VoiceChannelId, false));
        }

        /// <inheritdoc />
        public async ValueTask<(TLavaTrack Skipped, TLavaTrack Current)> SkipAsync(TimeSpan? skipAfter = default) {
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
            }

            if (!Queue.TryDequeue(out var lavaTrack)) {
                throw new InvalidOperationException("There aren't any more tracks in the queue.");
            }

            var skippedTrack = Track;
            await await Task.Delay(skipAfter ?? TimeSpan.Zero)
                .ContinueWith(_ => PlayAsync(lavaTrack, false));

            return (skippedTrack, lavaTrack);
        }

        /// <inheritdoc />
        public ValueTask SeekAsync(TimeSpan seekPosition) {
            if (PlayerState == PlayerState.None) {
                throw new InvalidOperationException(
                    "Player's current state is set to None. Please make sure Player is connected to a voice channel.");
            }

            if (seekPosition.TotalMilliseconds > Track.Duration.TotalMilliseconds) {
                throw new ArgumentOutOfRangeException(nameof(seekPosition), "");
            }

            return _webSocketClient.SendAsync(new SeekPayload(VoiceChannelId, seekPosition));
        }

        /// <inheritdoc />
        public ValueTask SetVolumeAsync(int volume) {
            return volume switch {
                < 0 => throw new ArgumentOutOfRangeException(nameof(volume),
                    "Volume must be greater than or equal to 0."),
                > 1000 => throw new ArgumentOutOfRangeException(nameof(volume),
                    "Volume must be less than or equal to 1000."),
                _ => _webSocketClient.SendAsync(new VolumePayload(VoiceChannelId, Volume = volume))
            };
        }

        /// <inheritdoc />
        public ValueTask EqualizeAsync(params EqualizerBand[] equalizerBands) {
            foreach (var band in equalizerBands) {
                _bands[band.Band] = band.Gain;
            }

            return _webSocketClient.SendAsync(new EqualizerPayload(VoiceChannelId, equalizerBands));
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() {
            await StopAsync();

            Queue.Clear();
        }
    }
}