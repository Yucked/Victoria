using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Victoria.Objects;
using Victoria.Payloads;

namespace Victoria
{
    public sealed class LavaPlayer
    {
        private bool IsDisposed;
        private LavaSocket _lavaSocket;

        /// <summary>
        ///     Connected Voice Channel.
        /// </summary>
        public IVoiceChannel VoiceChannel { get; internal set; }

        /// <summary>
        ///     Guild That Belongs To The Voice Channel.
        /// </summary>
        public IGuild Guild => VoiceChannel.Guild;

        /// <summary>
        ///     Text Channel That Handles Updates.
        /// </summary>
        public IMessageChannel TextChannel { get; }

        /// <summary>
        ///     Current Track Position.
        /// </summary>
        public TimeSpan Position { get; internal set; }

        /// <summary>
        ///     Track That Is Currently Playing.
        /// </summary>
        public LavaTrack CurrentTrack { get; internal set; }

        /// <summary>
        ///     Last Time This LavaPlayer Was Updated.
        /// </summary>
        public DateTimeOffset LastUpdate { get; internal set; }

        /// <summary>
        ///     If This LavaPlayer Is Connected Or Not.
        /// </summary>
        public bool IsConnected => !Volatile.Read(ref IsDisposed);

        /// <summary>
        ///     Default Queue That Stores Your Tracks.
        /// </summary>
        public ConcurrentDictionary<ulong, LinkedList<LavaTrack>> Queue { get; }

        internal LavaPlayer()
        {
        }

        internal LavaPlayer(LavaNode lavaNode, IVoiceChannel voiceChannel, IMessageChannel textChannel)
        {
            TextChannel = textChannel;
            VoiceChannel = voiceChannel;
            _lavaSocket = lavaNode.LavaSocket;
            Volatile.Write(ref IsDisposed, false);
            Queue = new ConcurrentDictionary<ulong, LinkedList<LavaTrack>>();
        }

        internal async Task DisconnectAsync()
        {
            if (!IsConnected)
                throw new InvalidOperationException("This player isn't connected.");
            CurrentTrack = null;
            await VoiceChannel.DisconnectAsync();
            Volatile.Write(ref IsDisposed, true);
            _lavaSocket.SendPayload(new DestroyPayload(Guild.Id));
        }

        /// <summary>
        ///     Plays The Specified Track.
        /// </summary>
        /// <param name="track">
        ///     <see cref="LavaTrack" />
        /// </param>
        /// <exception cref="InvalidOperationException">Throws If LavaPlayer Isn't Connected.</exception>
        public void Play(LavaTrack track)
        {
            if (!IsConnected)
                throw new InvalidOperationException("This player isn't connected.");
            CurrentTrack = track;
            _lavaSocket.SendPayload(new PlayPayload(Guild.Id, track));
        }

        /// <summary>
        ///     Plays The Specified Track But With A Specified Start And Stop Time.
        /// </summary>
        /// <param name="track">
        ///     <see cref="LavaTrack" />
        /// </param>
        /// <param name="start">When Track Should Start Playing.</param>
        /// <param name="stop">When Track Should Stop Playing.</param>
        /// <exception cref="InvalidOperationException">Throws If LavaPlayer Isn't Connected.</exception>
        /// <exception cref="ArgumentException">Throws If
        ///     <param name="start" />
        ///     and
        ///     <param name="stop" />
        ///     Aren't Set Properly.
        /// </exception>
        public void PlayPartial(LavaTrack track, TimeSpan start, TimeSpan stop)
        {
            if (!IsConnected)
                throw new InvalidOperationException("This player isn't connected.");

            if (start.TotalMilliseconds < 0 || stop.TotalMilliseconds < 0)
                throw new ArgumentException("Start & Stop Must Be Greater Than 0.");

            if (stop <= start)
                throw new ArgumentException("End Time Must Be Greater Than Start Time.");

            CurrentTrack = track;
            _lavaSocket.SendPayload(new PlayPartialPayload(Guild.Id, track, start, stop));
        }

        /// <summary>
        ///     Stop Playing Current Track A.K.A Skip.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws If Track Isn't Connected.</exception>
        public void Stop()
        {
            if (!IsConnected)
                throw new InvalidOperationException("This player isn't connected.");
            Volatile.Write(ref IsDisposed, true);
            CurrentTrack = null;
            _lavaSocket.SendPayload(new StopPayload(Guild.Id));
        }

        /// <summary>
        ///     Pauses The Current Track.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws If LavaPlayer Isn't Connected.</exception>
        public void Pause()
        {
            if (!IsConnected)
                throw new InvalidOperationException("This player isn't connected.");
            _lavaSocket.SendPayload(new PausePayload(true, Guild.Id));
        }

        /// <summary>
        ///     Resumes The Current Track.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws If LavaPlayer Isn't Connected.</exception>
        public void Resume()
        {
            if (!IsConnected)
                throw new InvalidOperationException("This player isn't connected.");
            _lavaSocket.SendPayload(new PausePayload(false, Guild.Id));
        }

        /// <summary>
        ///     Seeks The Current Track To Specified Time.
        /// </summary>
        /// <param name="position">Where To Skip To.</param>
        /// <exception cref="InvalidOperationException">Throws If LavaPlayer Isn't Connected.</exception>
        public void Seek(TimeSpan position)
        {
            if (!IsConnected)
                throw new InvalidOperationException("This player isn't connected.");
            _lavaSocket.SendPayload(new SeekPayload(position, Guild.Id));
        }

        /// <summary>
        ///     Set The Current LavaPlayer's Volume.
        /// </summary>
        /// <param name="volume"></param>
        /// <exception cref="InvalidOperationException">Throws If LavaPlayer Isn't Connected.</exception>
        /// <exception cref="ArgumentException"></exception>
        public void Volume(int volume)
        {
            if (!IsConnected)
                throw new InvalidOperationException("This player isn't connected.");

            if (volume < 0 || volume > 150)
                throw new ArgumentException("Volume range must be between 0 - 150.", nameof(volume));

            _lavaSocket.SendPayload(new VolumePayload(volume, Guild.Id));
        }

        /// <summary>
        ///     Add A Track To The Default <see cref="Queue" />.
        /// </summary>
        /// <param name="track">
        ///     <see cref="LavaTrack" />
        /// </param>
        public void Enqueue(LavaTrack track)
        {
            Queue[Guild.Id]?.AddLast(track);
        }

        /// <summary>
        ///     Enqueues Bunch of Tracks.
        /// </summary>
        /// <param name="tracks">
        ///     <see cref="LavaTrack" />
        /// </param>
        public void Enqueue(params LavaTrack[] tracks)
        {
            foreach (var track in tracks)
                Queue[Guild.Id]?.AddLast(track);
        }

        /// <summary>
        ///     Enqueues Bunch of Tracks.
        /// </summary>
        /// <param name="tracks">
        ///     <see cref="LavaTrack" />
        /// </param>
        public void Enqueue(IEnumerable<LavaTrack> tracks)
        {
            foreach (var track in tracks)
                Queue[Guild.Id]?.AddLast(track);
        }

        /// <summary>
        ///     Removes A Track From The Default <see cref="Queue" />.
        /// </summary>
        /// <param name="track">
        ///     <see cref="LavaTrack" />
        /// </param>
        public void Dequeue(LavaTrack track)
        {
            Queue[Guild.Id]?.Remove(track);
        }

        /// <summary>
        ///     Dequeues Bunch of Tracks.
        /// </summary>
        /// <param name="tracks"></param>
        public void Dequeue(params LavaTrack[] tracks)
        {
            foreach (var track in tracks)
                Queue[Guild.Id]?.Remove(track);
        }

        /// <summary>
        ///     Dequeues Bunch of Tracks.
        /// </summary>
        /// <param name="tracks"></param>
        public void Dequeue(IEnumerable<LavaTrack> tracks)
        {
            foreach (var track in tracks)
                Queue[Guild.Id]?.Remove(track);
        }
    }
}