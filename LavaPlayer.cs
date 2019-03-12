using Discord;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Entities;
using Victoria.Entities.Payloads;
using Victoria.Helpers;
using Victoria.Queue;

namespace Victoria
{
    /// <summary>
    /// Represents a <see cref="IVoiceChannel"/> connection.
    /// </summary>
    public sealed class LavaPlayer
    {
        /// <summary>
        /// Keeps track of <see cref="PauseAsync"/> & <see cref="ResumeAsync"/>.
        /// </summary>
        public bool IsPaused => isPaused;

        /// <summary>
        /// Checks whether the <see cref="LavaPlayer"/> is playing or not.
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Current track that is playing.
        /// </summary>
        public LavaTrack CurrentTrack { get; internal set; }

        /// <summary>
        /// Optional text channel.
        /// </summary>
        public ITextChannel TextChannel { get; }

        /// <summary>
        /// Connected voice channel.
        /// </summary>
        public IVoiceChannel VoiceChannel { get; }

        /// <summary>
        /// Default queue, takes an object that implements <see cref="IQueueObject"/>.
        /// </summary>
        public LavaQueue<IQueueObject> Queue { get; private set; }

        /// <summary>
        /// Last time when Lavalink sent an updated.
        /// </summary>
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <summary>
        /// Keeps track of volume set by <see cref="SetVolumeAsync(int)"/>;
        /// </summary>
        public int CurrentVolume { get; private set; }

        private bool isPaused;
        private readonly SocketHelper _socketHelper;

        private const string InvalidOp
            = "This operation is invalid since player isn't actually playing anything.";

        internal LavaPlayer(IVoiceChannel voiceChannel, ITextChannel textChannel,
            SocketHelper socketHelper)
        {
            VoiceChannel = voiceChannel;
            TextChannel = textChannel;
            _socketHelper = socketHelper;
            Queue = new LavaQueue<IQueueObject>();
        }

        /// <summary>
        /// Plays the specified <paramref name="track"/>.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        /// <param name="noReplace">If set to true, this operation will be ignored if a track is already playing or paused.</param>
        public Task PlayAsync(LavaTrack track, bool noReplace = false)
        {
            IsPlaying = true;
            CurrentTrack = track;
            var payload = new PlayPayload(VoiceChannel.GuildId, track.Hash, noReplace);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Plays the specified <paramref name="track"/>.
        /// </summary>
        /// <param name="track"></param>
        /// <param name="startTime">Optional setting that determines the number of milliseconds to offset the track by.</param>
        /// <param name="stopTime">optional setting that determines at the number of milliseconds at which point the track should stop playing.</param>
        /// <param name="noReplace">If set to true, this operation will be ignored if a track is already playing or paused.</param>
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
        /// Stops playing the current track and sets <see cref="IsPlaying"/> to false.
        /// </summary>
        public Task StopAsync()
        {
            if (!IsPlaying)
                throw new InvalidOperationException(InvalidOp);

            IsPlaying = false;
            CurrentTrack = null;
            var payload = new StopPayload(VoiceChannel.GuildId);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Resumes if <see cref="IsPaused"/> is set to true.
        /// </summary>
        public Task ResumeAsync()
        {
            if (!IsPlaying)
                throw new InvalidOperationException(InvalidOp);

            Volatile.Write(ref isPaused, false);
            var payload = new PausePayload(VoiceChannel.GuildId, IsPaused);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Pauses if <see cref="IsPaused"/> is set to false.
        /// </summary>
        public Task PauseAsync()
        {
            if (!IsPlaying)
                throw new InvalidOperationException(InvalidOp);

            Volatile.Write(ref isPaused, true);
            var payload = new PausePayload(VoiceChannel.GuildId, IsPaused);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Replaces the <see cref="CurrentTrack"/> with the next <see cref="LavaTrack"/> from <see cref="Queue"/>.
        /// </summary>
        /// <returns>Returns the skipped <see cref="LavaTrack"/>.</returns>
        public async Task<LavaTrack> SkipAsync()
        {
            if (!Queue.TryDequeue(out var item))
                throw new InvalidOperationException($"There are no more items in {nameof(Queue)}.");

            if (!(item is LavaTrack track))
                throw new InvalidCastException($"Couldn't cast {item.GetType()} to {typeof(LavaTrack)}.");

            var previousTrack = CurrentTrack;
            await PlayAsync(track);
            return previousTrack;
        }

        /// <summary>
        /// Seeks the <see cref="CurrentTrack"/> to specified <paramref name="position"/>.
        /// </summary>
        /// <param name="position">Position must be less than <see cref="CurrentTrack"/>'s position.</param>
        public Task SeekAsync(TimeSpan position)
        {
            if (!IsPlaying)
                throw new InvalidOperationException(InvalidOp);

            if (position > CurrentTrack.Length)
                throw new ArgumentOutOfRangeException($"{nameof(position)} is greater than current track's length.");

            var payload = new SeekPayload(VoiceChannel.GuildId, position);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Updates <see cref="LavaPlayer"/> volume and updates <see cref="CurrentVolume"/>.
        /// </summary>
        /// <param name="volume">Volume may range from 0 to 1000. 100 is default.</param>
        public Task SetVolumeAsync(int volume)
        {
            if (!IsPlaying)
                throw new InvalidOperationException(InvalidOp);

            if (volume > 1000)
                throw new ArgumentOutOfRangeException($"{nameof(volume)} was greater than max limit which is 1000.");

            CurrentVolume = volume;
            var payload = new VolumePayload(VoiceChannel.GuildId, volume);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Change the <see cref="LavaPlayer"/>'s equalizer. There are 15 bands (0-14) that can be changed.
        /// </summary>
        /// <param name="bands"><see cref="EqualizerBand"/></param>
        public Task EqualizerAsync(List<EqualizerBand> bands)
        {
            if (!IsPlaying)
                throw new InvalidOperationException(InvalidOp);

            var payload = new EqualizerPayload(VoiceChannel.GuildId, bands);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Change the <see cref="LavaPlayer"/>'s equalizer. There are 15 bands (0-14) that can be changed.
        /// </summary>
        /// <param name="bands"><see cref="EqualizerBand"/></param>
        public Task EqualizerAsync(params EqualizerBand[] bands)
        {
            if (!IsPlaying)
                throw new InvalidOperationException(InvalidOp);

            var payload = new EqualizerPayload(VoiceChannel.GuildId, bands);
            return _socketHelper.SendPayloadAsync(payload);
        }

        /// <summary>
        /// Disposes <see cref="LavaPlayer"/>, sends a stop and destroy request to Lavalink server and disconnects from <see cref="VoiceChannel"/>.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            IsPlaying = false;
            Queue.Clear();
            Queue = null;
            CurrentTrack = null;
            var stopPayload = new StopPayload(VoiceChannel.GuildId);
            var destroyPayload = new DestroyPayload(VoiceChannel.GuildId);
            await _socketHelper.SendPayloadAsync(stopPayload);
            await _socketHelper.SendPayloadAsync(destroyPayload);
            await VoiceChannel.DisconnectAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}
