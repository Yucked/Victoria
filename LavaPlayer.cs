using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Victoria.Entities;
using Victoria.Entities.Payloads;

namespace Victoria
{
    /// <summary>
    /// Represents a voice channel connection.
    /// </summary>
    public sealed class LavaPlayer
    {
        /// <summary>
        /// Volume of current player.
        /// </summary>
        public int Volume { get; private set; }

        /// <summary>
        /// Whether this player is playing any tracks.
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Checks if the player is paused or resumed.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Current track that is being played.
        /// </summary>
        public LavaTrack CurrentTrack { get; internal set; }

        /// <summary>
        /// Last time this player was updated.
        /// </summary>
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <summary>
        /// Voice channel this player is connected to.
        /// </summary>
        public IVoiceChannel VoiceChannel { get; internal set; }

        /// <summary>
        /// Text channel this player is bound to.
        /// </summary>
        public IMessageChannel TextChannel { get; set; }

        /// <summary>
        /// Default queue.
        /// </summary>
        public LavaQueue<LavaTrack> Queue { get; }

        private readonly LavaNode _lavaNode;
        private bool IsAvailable => IsPlaying && CurrentTrack != null;
        private const string InvalidOpMessage = "Can't perform this operation, player isn't being used.";

        internal LavaPlayer()
        {
        }

        internal LavaPlayer(LavaNode lavaNode, IVoiceChannel voiceChannel, IMessageChannel messageChannel)
        {
            Volume = 100;
            _lavaNode = lavaNode;
            VoiceChannel = voiceChannel;
            TextChannel = messageChannel;
            Queue = new LavaQueue<LavaTrack>();
        }

        /// <summary>
        /// Plays the given track.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        /// <param name="replace">If set to true, this operation will be ignored if a track is already playing or paused</param>
        public async Task PlayAsync(LavaTrack track, bool replace = true)
        {
            IsPlaying = true;
            CurrentTrack = track;
            var payload = new PlayPayload(track.TrackString, TimeSpan.Zero, track.Length, replace,
                VoiceChannel.GuildId);
            await _lavaNode.Socket.SendPayloadAsync(payload).ConfigureAwait(false);
        }

        /// <summary>
        /// Partially plays the given track.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        /// <param name="startTime">Start time of the track.</param>
        /// <param name="stopTime">Stop time of the track.</param>
        /// <param name="replace">If set to true, this operation will be ignored if a track is already playing or paused</param>
        public async Task PlayAsync(LavaTrack track, TimeSpan startTime, TimeSpan stopTime, bool replace = true)
        {
            if (startTime.TotalMilliseconds < 0 || stopTime.TotalMilliseconds < 0)
                throw new InvalidOperationException("Start and stop must be greater than 0.");

            if (startTime <= stopTime)
                throw new InvalidOperationException("Stop time must be greater than start time.");

            CurrentTrack = track;
            var payload = new PlayPayload(track.TrackString, startTime, stopTime, replace,
                VoiceChannel.GuildId);
            await _lavaNode.Socket.SendPayloadAsync(payload).ConfigureAwait(false);
        }

        /// <summary>
        /// Skips the current track that is playing and plays the next song from <see cref="Queue"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if player isn't playing anything and current track is null.</exception>
        public async Task<LavaTrack> SkipAsync()
        {
            if (!IsAvailable)
                throw new InvalidOperationException(InvalidOpMessage);

            if (!Queue.TryDequeue(out var track))
            {
                await StopAsync().ConfigureAwait(false);
                throw new InvalidOperationException("Couldn't find anything to play since queue was empty.");
            }

            await PlayAsync(track).ConfigureAwait(false);
            return track;
        }

        /// <summary>
        /// Stops playing the current track.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if player isn't playing anything and current track is null.</exception>
        public async Task StopAsync()
        {
            if (!IsAvailable)
                throw new InvalidOperationException(InvalidOpMessage);
            CurrentTrack = null;
            IsPlaying = false;
            await _lavaNode.Socket.SendPayloadAsync(new StopPayload(VoiceChannel.GuildId)).ConfigureAwait(false);
        }

        /// <summary>
        /// Pauses or resumes toe player.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if player isn't playing anything and current track is null.</exception>
        public async Task PauseAsync()
        {
            if (!IsAvailable)
                throw new InvalidOperationException(InvalidOpMessage);
            IsPaused = !IsPaused;
            await _lavaNode.Socket.SendPayloadAsync(new PausePayload(IsPaused, VoiceChannel.GuildId))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Make the player seek to a certain position.
        /// </summary>
        /// <param name="position"><see cref="TimeSpan"/></param>
        /// <exception cref="InvalidOperationException">Throws if player isn't playing anything and current track is null.</exception>
        public async Task SeekAsync(TimeSpan position)
        {
            if (!IsAvailable)
                throw new InvalidOperationException(InvalidOpMessage);

            await _lavaNode.Socket.SendPayloadAsync(new SeekPayload(position, VoiceChannel.GuildId))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Changes the player equalizer bands.
        /// </summary>
        /// <param name="bands">List of bands ranging from 0 - 14.</param>
        /// <exception cref="InvalidOperationException">Throws if player isn't playing anything and current track is null.</exception>
        public async Task EqualizerAsync(List<EqualizerBand> bands)
        {
            if (!IsAvailable)
                throw new InvalidOperationException(InvalidOpMessage);
            await _lavaNode.Socket.SendPayloadAsync(new EqualizerPayload(VoiceChannel.GuildId, bands))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Changes the player equalizer bands.
        /// </summary>
        /// <param name="bands">List of bands ranging from 0 - 14.</param>
        /// <exception cref="InvalidOperationException">Throws if player isn't playing anything and current track is null.</exception>
        public async Task EqualizerAsync(params EqualizerBand[] bands)
        {
            if (!IsAvailable)
                throw new InvalidOperationException(InvalidOpMessage);
            await _lavaNode.Socket.SendPayloadAsync(new EqualizerPayload(VoiceChannel.GuildId, bands))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Changes player volume.
        /// </summary>
        /// <param name="volume"></param>
        /// <exception cref="InvalidOperationException">Throws if player isn't connected.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws if volume is greater than 150.</exception>
        public async Task SetVolumeAsync(int volume)
        {
            if (!IsAvailable)
                throw new InvalidOperationException(InvalidOpMessage);

            if (volume > 150)
                throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be lower than 150.");

            Volume = volume;
            await _lavaNode.Socket.SendPayloadAsync(new VolumePayload(volume, VoiceChannel.GuildId))
                .ConfigureAwait(false);
        }

        internal void Dispose()
        {
            Volume = 0;
            IsPlaying = false;
            CurrentTrack = null;
            Queue.Clear();
        }
    }
}