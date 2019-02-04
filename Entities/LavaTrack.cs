using System;
using Victoria.Queue;

namespace Victoria.Entities
{
    public class LavaTrack : IQueueObject
    {
        public string Title { get; internal set; }
        public string Author { get; internal set; }
        public TimeSpan Position { get; internal set; }
        public string Url { get; internal set; }
        public string Id { get; internal set; }

        public void ResetPosition()
        {
            Position = TimeSpan.Zero;
        }
    }
}