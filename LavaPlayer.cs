using Discord;
using System;
using System.Threading.Tasks;
using Victoria.Entities.Responses.LoadTracks;
using Victoria.Queue;

namespace Victoria
{
    public class LavaPlayer : IAsyncDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsPaused { get; }

        /// <summary>
        /// 
        /// </summary>
        public Track CurrentTrack { get; }

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


        internal LavaPlayer()
        {
            Queue = new LavaQueue<IQueueObject>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public async Task PlayAsync(Track track)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask<Track> SkipAsync()
        {
            if (!Queue.TryDequeue(out var item))
                throw new InvalidOperationException("");

            if (!(item is Track track))
                throw new ArgumentException("");

            await StopAsync();
            await PlayAsync(track);
            return track;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            Queue.Clear();
            await VoiceChannel.DisconnectAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}