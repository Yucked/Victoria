using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Victoria.Entities;
using Victoria.Entities.Payloads;

namespace Victoria
{
    public sealed class LavaPlayer
    {
        /// <summary>
        /// Volume of current player.
        /// </summary>
        public ushort Volume { get; private set; }

        /// <summary>
        /// Whether this player is playing any tracks.
        /// </summary>
        public bool IsPlaying { get; private set; }

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
        public IMessageChannel MessageChannel { get; }

        /// <summary>
        /// Default queue.
        /// </summary>
        public LavaQueue<LavaTrack> Queue { get; }

        private readonly LavaNode _lavaNode;
        private bool IsAvailable => IsPlaying && CurrentTrack != null;
        private const string invalidOpMessage = "Can't perform this operation, player isn't being used.";

        internal LavaPlayer()
        {
        }

        internal LavaPlayer(LavaNode lavaNode, IVoiceChannel voiceChannel, IMessageChannel messageChannel)
        {
            Volume = 100;
            _lavaNode = lavaNode;
            VoiceChannel = voiceChannel;
            MessageChannel = messageChannel;
            Queue = new LavaQueue<LavaTrack>();
        }

        /// <summary>
        /// Disconnects from the voicechannel and disposes the player completely.
        /// </summary>
        public async Task DisconnectAsync()
        {
            Dispose();
            await _lavaNode._socket.SendPayloadAsync(new DestroyPayload(VoiceChannel.GuildId)).ConfigureAwait(false);
            await VoiceChannel.DisconnectAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Plays the given track.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        public async Task PlayAsync(LavaTrack track)
        {
            CurrentTrack = track;
            IsPlaying = true;
            await _lavaNode._socket.SendPayloadAsync(new PlayPayload(track.TrackString, VoiceChannel.GuildId))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Partially plays the given track.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        /// <param name="startTime">Start time of the track.</param>
        /// <param name="stopTime">Stop time of the track.</param>
        public async Task PlayAsync(LavaTrack track, TimeSpan startTime, TimeSpan stopTime)
        {
            if (startTime.TotalMilliseconds < 0 || stopTime.TotalMilliseconds < 0)
                throw new InvalidOperationException("Start and stop must be greater than 0.");

            if (startTime <= stopTime)
                throw new InvalidOperationException("Stop time must be greater than start time.");

            CurrentTrack = track;
            await _lavaNode._socket
                .SendPayloadAsync(new PlayPartialPayload(track.TrackString, startTime, stopTime, VoiceChannel.GuildId))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Skips the current track that is playing and plays the next song from <see cref="Queue"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if player isn't playing anything and current track is null.</exception>
        public async Task<LavaTrack> SkipAsync()
        {
            if (!IsAvailable)
                throw new InvalidOperationException(invalidOpMessage);

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
                throw new InvalidOperationException(invalidOpMessage);
            CurrentTrack = null;
            IsPlaying = false;
            await _lavaNode._socket.SendPayloadAsync(new StopPayload(VoiceChannel.GuildId)).ConfigureAwait(false);
        }

        /// <summary>
        /// Pauses the player.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if player isn't playing anything and current track is null.</exception>
        public async Task PauseAsync()
        {
            if (!IsAvailable)
                throw new InvalidOperationException(invalidOpMessage);

            await _lavaNode._socket.SendPayloadAsync(new PausePayload(true, VoiceChannel.GuildId))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Resumes the player.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if player isn't playing anything and current track is null.</exception>
        public async Task ResumeAsync()
        {
            if (!IsAvailable)
                throw new InvalidOperationException(invalidOpMessage);

            await _lavaNode._socket.SendPayloadAsync(new PausePayload(false, VoiceChannel.GuildId))
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
                throw new InvalidOperationException(invalidOpMessage);

            await _lavaNode._socket.SendPayloadAsync(new SeekPayload(position, VoiceChannel.GuildId))
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
                throw new InvalidOperationException(invalidOpMessage);
            await _lavaNode._socket.SendPayloadAsync(new EqualizerPayload(VoiceChannel.GuildId, bands))
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
                throw new InvalidOperationException(invalidOpMessage);
            await _lavaNode._socket.SendPayloadAsync(new EqualizerPayload(VoiceChannel.GuildId, bands))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Changes player volume.
        /// </summary>
        /// <param name="volume"></param>
        /// <exception cref="InvalidOperationException">Throws if player isn't connected.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws if volume is greater than 150.</exception>
        public async Task SetVolumeAsync(ushort volume)
        {
            if (!IsAvailable)
                throw new InvalidOperationException(invalidOpMessage);

            if (volume > 150)
                throw new ArgumentOutOfRangeException(nameof(volume), "Volume must be lower than 150.");

            Volume = volume;
            await _lavaNode._socket.SendPayloadAsync(new VolumePayload(volume, VoiceChannel.GuildId))
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