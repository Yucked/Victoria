using Discord;
using System;
using System.Threading.Tasks;
using Victoria.Entities.Responses;
using Victoria.Queue;

namespace Victoria
{
    public class LavaPlayer :
#if C3
        IAsyncDisposable
#else
          IDisposable
#endif

    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsPaused { get; }

        /// <summary>
        /// 
        /// </summary>
        public LavaTrack CurrentTrack { get; }

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
        public async Task PlayAsync(LavaTrack track)
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


        #region DISPOSE
        private async Task DisposeResources()
        {
            Queue.Clear();
            await VoiceChannel.DisconnectAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

#if C3
        public ValueTask DisposeAsync()
        {
            return new ValueTask(DisposeResources());
        }
#else
        public void Dispose()
        {
            DisposeResources().GetAwaiter().GetResult();
        }
#endif
        #endregion
    }
}