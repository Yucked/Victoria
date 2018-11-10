using Discord;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Objects;
using Victoria.Payloads;

namespace Victoria
{
    public sealed class LavaPlayer
    {
        private bool _isDisposed;
        private readonly LavaSocket _lavaSocket;

        /// <summary>
        /// Voice channel that the player is connected to.
        /// </summary>
        public IVoiceChannel VoiceChannel { get; internal set; }

        /// <summary>
        /// Guild that the player belongs to.
        /// </summary>
        public IGuild Guild => VoiceChannel.Guild;

        /// <summary>
        /// Text channel linked to player.
        /// </summary>
        public IMessageChannel TextChannel { get; private set; }

        /// <summary>
        /// Current track positon.
        /// </summary>
        public TimeSpan Position { get; internal set; }

        /// <summary>
        /// Current track that is playing.
        /// </summary>
        public LavaTrack CurrentTrack { get; internal set; }

        /// <summary>
        /// Last time when player was updated.
        /// </summary>
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <summary>
        /// If player is playing any songs or connected.
        /// </summary>
        public bool IsConnected => !Volatile.Read(ref _isDisposed);

        /// <summary>
        /// Default queue.
        /// </summary>
        //public ConcurrentQueue<LavaTrack> Queue { get; internal set; }
        public LavaQueue<LavaTrack> Queue { get; internal set; }

        internal LavaPlayer()
        {
        }

        internal LavaPlayer(LavaNode lavaNode, IVoiceChannel voiceChannel, IMessageChannel textChannel)
        {
            TextChannel = textChannel;
            VoiceChannel = voiceChannel;
            _lavaSocket = lavaNode.LavaSocket;
            Volatile.Write(ref _isDisposed, false);
            Queue = new LavaQueue<LavaTrack>();
        }

        internal async Task DisconnectAsync()
        {
            await VoiceChannel.DisconnectAsync();
            Dispose();
        }

        /// <summary>
        /// Plays the specified track.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        /// <exception cref="InvalidOperationException">Throws if player isn't connected.</exception>
        public void Play(LavaTrack track)
        {
            CurrentTrack = track;
            Volatile.Write(ref _isDisposed, false);
            _lavaSocket.SendPayload(new PlayPayload(Guild.Id, track));
        }

        /// <summary>
        /// Play a track with start and stop time.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        /// <param name="start">Start time for track.</param>
        /// <param name="stop">Stop time for track.</param>
        /// <exception cref="InvalidOperationException">Throws if player isn't connected.</exception>
        /// <exception cref="ArgumentException">Throws if start and stop logic isn't valid.</exception>
        public void PlayPartial(LavaTrack track, TimeSpan start, TimeSpan stop)
        {
            if (start.TotalMilliseconds < 0 || stop.TotalMilliseconds < 0)
                throw new ArgumentException("Start & Stop Must Be Greater Than 0.");

            if (stop <= start)
                throw new ArgumentException("End Time Must Be Greater Than Start Time.");

            CurrentTrack = track;
            Volatile.Write(ref _isDisposed, false);
            _lavaSocket.SendPayload(new PlayPartialPayload(Guild.Id, track, start, stop));
        }


        /// <summary>
        /// Skips a song. (Works only with default queue).
        /// </summary>
        /// <returns>Returns the next <see cref="LavaTrack"/></returns>
        /// <exception cref="InvalidOperationException">Throws if <see cref="Queue"/> is empty or if player isn't connected.</exception>
        public LavaTrack Skip()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Either this player isn't connected or connection isn't valid.");

            if (!Queue.TryDequeue(out var track))
            {
                Stop();
                throw new InvalidOperationException("Queue was empty. Player has been stopped.");
            }

            Play(track);
            return track;
        }

        /// <summary>
        /// Stops the player completely and sets <see cref="CurrentTrack"/> to null.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if player isn't connected.</exception>
        public void Stop()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Either this player isn't connected or connection isn't valid.");
            CurrentTrack = null;
            Volatile.Write(ref _isDisposed, true);
            _lavaSocket.SendPayload(new StopPayload(Guild.Id));
        }

        /// <summary>
        /// Pauses the current player.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if player isn't connected.</exception>
        public void Pause()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Either this player isn't connected or connection isn't valid.");
            _lavaSocket.SendPayload(new PausePayload(true, Guild.Id));
        }

        /// <summary>
        /// Resumes the current player.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if player isn't connected.</exception>
        public void Resume()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Either this player isn't connected or connection isn't valid.");
            _lavaSocket.SendPayload(new PausePayload(false, Guild.Id));
        }

        /// <summary>
        /// Seeks the current track to specific time frame.
        /// </summary>
        /// <param name="position">Where To Skip To.</param>
        /// <exception cref="InvalidOperationException">Throws if player isn't connected.</exception>
        public void Seek(TimeSpan position)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Either this player isn't connected or connection isn't valid.");
            _lavaSocket.SendPayload(new SeekPayload(position, Guild.Id));
        }

        /// <summary>
        /// Sets volume of current player.
        /// </summary>
        /// <param name="volume"></param>
        /// <exception cref="InvalidOperationException">Throws if player isn't connected.</exception>
        /// <exception cref="ArgumentException">Throws if volume is out of range.</exception>
        public void Volume(int volume)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Either this player isn't connected or connection isn't valid.");

            if (volume < 0 || volume > 150)
                throw new ArgumentOutOfRangeException(nameof(volume), "Value must be between 0 - 150.");

            _lavaSocket.SendPayload(new VolumePayload(volume, Guild.Id));
        }

        /// <summary>
        /// Changes the player equalizer.
        /// </summary>
        /// <param name="equalizerBands">List of bands ranging from 0 - 14.</param>
        /// <exception cref="InvalidOperationException">Throws if player isn't connected.</exception>
        public void Equalizer(params EqualizerBand[] equalizerBands)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Either this player isn't connected or connection isn't valid.");

            _lavaSocket.SendPayload(new EqualizerPayload(Guild.Id, equalizerBands));
        }

        /// <summary>
        /// Adds a track to default queue.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        public void Enqueue(LavaTrack track)
        {
            Queue.Enqueue(track);
        }

        /// <summary>
        /// Adds multiple tracks to default queue.
        /// </summary>
        /// <param name="tracks"><see cref="LavaTrack"/></param>
        public void Enqueue(IEnumerable<LavaTrack> tracks)
        {
            foreach (var track in tracks)
                Queue.Enqueue(track);
        }

        /// <summary>
        /// Dequeues the first track from the <see cref="Queue"/>.
        /// </summary>
        /// <returns><see cref="LavaTrack"/></returns>
        public LavaTrack Dequeue()
        {
            return Queue.Dequeue();
        }

        /// <summary>
        /// Removes the first instance of given <see cref="LavaTrack"/>.
        /// </summary>
        /// <param name="track"><see cref="LavaTrack"/></param>
        public void Remove(LavaTrack track)
        {
            Queue.Remove(track);
        }

        private void Dispose()
        {
            _lavaSocket.SendPayload(new DestroyPayload(Guild.Id));
            VoiceChannel = null;
            TextChannel = null;
            CurrentTrack = null;
            Position = TimeSpan.MinValue;
            LastUpdate = DateTime.Now;
            Queue = null;
            Volatile.Write(ref _isDisposed, true);
        }
    }
}