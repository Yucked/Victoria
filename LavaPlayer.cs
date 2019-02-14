using Discord;
using System;
using System.Threading.Tasks;
using Victoria.Entities.Responses.LoadTracks;
using Victoria.Queue;

namespace Victoria
{
    public class LavaPlayer : IAsyncDisposable
    {
        public bool IsPaused { get; }

        public Track CurrentTrack { get; }

        public ITextChannel TextChannel { get; }

        public IVoiceChannel VoiceChannel { get; }

        public LavaQueue<IQueueObject> Queue { get; }

        internal LavaPlayer()
        {
            Queue = new LavaQueue<IQueueObject>();
        }

        public async Task PlayAsync(Track track)
        {

        }

        public async Task StopAsync()
        {
        }

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

        public async ValueTask DisposeAsync()
        {
            Queue.Clear();
            await VoiceChannel.DisconnectAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}