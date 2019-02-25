using Discord;
using System;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Entities.Payloads;
using Victoria.Entities.Responses;
using Victoria.Helpers;
using Victoria.Queue;

namespace Victoria
{
    public class LavaPlayer
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsPaused => isPaused;
        private bool isPaused;

        /// <summary>
        /// 
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public LavaTrack CurrentTrack { get; private set; }

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
        public LavaQueue<IQueueObject> Queue { get; }

        /// <summary>
        /// 
        /// </summary>
        public int CurrentVolume { get; private set; }

        private readonly SocketHelper _socketHelper;

        internal LavaPlayer(IVoiceChannel voiceChannel, ITextChannel textChannel,
            SocketHelper socketHelper)
        {
            VoiceChannel = voiceChannel;
            TextChannel = textChannel;
            _socketHelper = socketHelper;
            Queue = new LavaQueue<IQueueObject>();
        }

        public Task PlayAsync(LavaTrack track, bool noReplace = false)
        {
            IsPlaying = true;
            CurrentTrack = track;
            var payload = new PlayPayload(VoiceChannel.GuildId, track.Hash, noReplace);
            return _socketHelper.SendPayloadAsync(payload);
        }

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

        public Task StopAsync()
        {
            if (!IsPlaying)
                throw new InvalidOperationException();

            IsPlaying = false;
            CurrentTrack = null;
            var payload = new StopPayload(VoiceChannel.GuildId);
            return _socketHelper.SendPayloadAsync(payload);
        }

        public Task ResumeAsync()
        {
            Volatile.Write(ref isPaused, false);
            var payload = new PausePayload(VoiceChannel.GuildId, IsPaused);
            return _socketHelper.SendPayloadAsync(payload);
        }

        public Task PauseAsync()
        {
            Volatile.Write(ref isPaused, true);
            var payload = new PausePayload(VoiceChannel.GuildId, IsPaused);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual async Task<LavaTrack> SkipAsync()
        {
            if (!Queue.TryDequeue(out var item))
                throw new InvalidOperationException("");

            if (!(item is LavaTrack track))
                throw new ArgumentException("");

            await StopAsync();
            await PlayAsync(track);
            return track;
        }

        public async ValueTask DisposeAsync()
        {
            Queue.Clear();
            await VoiceChannel.DisconnectAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}