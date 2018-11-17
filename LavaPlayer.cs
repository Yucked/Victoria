using System;
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
        /// Checks if current player is playing anything.
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
        /// 
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
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (!IsAvailable)
                throw new InvalidOperationException(invalidOpMessage);

            IsPlaying = false;
            CurrentTrack = null;
            await _lavaNode._socket.SendPayloadAsync(new DestroyPayload(VoiceChannel.GuildId)).ConfigureAwait(false);
            await VoiceChannel.DisconnectAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public async Task PlayAsync(LavaTrack track)
        {
            CurrentTrack = track;
            await _lavaNode._socket.SendPayloadAsync(new PlayPayload(track.TrackString, VoiceChannel.GuildId))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <param name="startTime"></param>
        /// <param name="stopTime"></param>
        /// <returns></returns>
        public async Task PlayAsync(LavaTrack track, TimeSpan startTime, TimeSpan stopTime)
        {
            CurrentTrack = track;
            await _lavaNode._socket
                .SendPayloadAsync(new PlayPartialPayload(track.TrackString, startTime, stopTime, VoiceChannel.GuildId))
                .ConfigureAwait(false);
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
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
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task StopAsync()
        {
            if (!IsAvailable)
                throw new InvalidOperationException(invalidOpMessage);
            CurrentTrack = null;
            IsPlaying = false;
            await _lavaNode._socket.SendPayloadAsync(new StopPayload(VoiceChannel.GuildId)).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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