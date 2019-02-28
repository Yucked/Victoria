using Discord;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Entities;
using Victoria.Entities.Payloads;
using Victoria.Entities.Responses;
using Victoria.Helpers;
using Victoria.Queue;

namespace Victoria
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LavaPlayer
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsPaused => isPaused;

        /// <summary>
        /// 
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public LavaTrack CurrentTrack { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public ITextChannel TextChannel { get; }

        /// <summary>
        /// 
        /// </summary>
        public IVoiceChannel VoiceChannel { get; }

        /// <summary>
        /// 
        /// </summary>
        public LavaQueue<IQueueObject> Queue { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public int CurrentVolume { get; private set; }

        private bool isPaused;
        private readonly SocketHelper _socketHelper;

        internal LavaPlayer(IVoiceChannel voiceChannel, ITextChannel textChannel,
            SocketHelper socketHelper)
        {
            VoiceChannel = voiceChannel;
            TextChannel = textChannel;
            _socketHelper = socketHelper;
            Queue = new LavaQueue<IQueueObject>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <param name="noReplace"></param>
        /// <returns></returns>
        public Task PlayAsync(LavaTrack track, bool noReplace = false)
        {
            IsPlaying = true;
            CurrentTrack = track;
            var payload = new PlayPayload(VoiceChannel.GuildId, track.Hash, noReplace);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <param name="startTime"></param>
        /// <param name="stopTime"></param>
        /// <param name="noReplace"></param>
        /// <returns></returns>
        public Task PlayAsync(LavaTrack track, TimeSpan startTime, TimeSpan stopTime, bool noReplace = false)
        {
            if (startTime.TotalMilliseconds < 0 || stopTime.TotalMilliseconds < 0)
                throw new InvalidOperationException("Start and stop must be greater than 0.");

            if (startTime <= stopTime)
                throw new InvalidOperationException("Stop time must be greater than start time.");

            IsPlaying = true;
            CurrentTrack = track;
            var payload = new PlayPayload(VoiceChannel.GuildId, track.Hash, startTime, stopTime, noReplace);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task StopAsync()
        {
            if (!IsPlaying)
                throw new InvalidOperationException();

            IsPlaying = false;
            CurrentTrack = null;
            var payload = new StopPayload(VoiceChannel.GuildId);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task ResumeAsync()
        {
            if (!IsPlaying)
                throw new InvalidOperationException();

            Volatile.Write(ref isPaused, false);
            var payload = new PausePayload(VoiceChannel.GuildId, IsPaused);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task PauseAsync()
        {
            if (!IsPlaying)
                throw new InvalidOperationException();

            Volatile.Write(ref isPaused, true);
            var payload = new PausePayload(VoiceChannel.GuildId, IsPaused);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<LavaTrack> SkipAsync()
        {
            if (!Queue.TryDequeue(out var item))
                throw new InvalidOperationException("");

            if (!(item is LavaTrack track))
                throw new InvalidCastException();

            await StopAsync();
            await PlayAsync(track);
            return track;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Task SeekAsync(TimeSpan position)
        {
            if (!IsPlaying)
                throw new InvalidOperationException();

            var payload = new SeekPayload(VoiceChannel.GuildId, position);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="volume"></param>
        /// <returns></returns>
        public Task SetVolumeAsync(int volume)
        {
            if (!IsPlaying)
                throw new InvalidOperationException();

            if (volume > 1000)
                throw new ArgumentOutOfRangeException();

            CurrentVolume = volume;
            var payload = new VolumePayload(VoiceChannel.GuildId, volume);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bands"></param>
        /// <returns></returns>
        public Task EqualizerAsync(List<EqualizerBand> bands)
        {
            if (!IsPlaying)
                throw new InvalidOperationException();

            var payload = new EqualizerPayload(VoiceChannel.GuildId, bands);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bands"></param>
        /// <returns></returns>
        public Task EqualizerAsync(params EqualizerBand[] bands)
        {
            if (!IsPlaying)
                throw new InvalidOperationException();

            var payload = new EqualizerPayload(VoiceChannel.GuildId, bands);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            IsPlaying = false;
            Queue.Clear();
            Queue = null;
            CurrentTrack = null;
            await VoiceChannel.DisconnectAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}