using System;
using System.Threading.Tasks;
using Victoria.Entities;
using Victoria.Queue;

namespace Victoria
{
    public class LavaPlayer : IDisposable
    {
        internal LavaPlayer()
        {
            Queue = new LavaQueue<IQueueObject>();
        }

        /// <summary>
        /// </summary>
        public LavaQueue<IQueueObject> Queue { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task PlayAsync(LavaTrack track)
        {
        }

        public async Task StopAsync()
        {
        }

        public virtual async ValueTask<LavaTrack> SkipAsync()
        {
            if (!Queue.TryDequeue(out var item))
                throw new InvalidOperationException("");

            if (!(item is LavaTrack track))
                throw new ArgumentException("");

            await StopAsync();
            await PlayAsync(track);
            return track;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Queue.Clear();
        }
    }
}