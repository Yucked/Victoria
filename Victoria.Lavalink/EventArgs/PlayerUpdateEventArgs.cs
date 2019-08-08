using System;
using Victoria.Lavalink.Responses.WebSocket;

namespace Victoria.Lavalink.EventArgs
{
    /// <summary>
    ///     Contains information about track position.
    /// </summary>
    public readonly struct PlayerUpdateEventArgs
    {
        /// <summary>
        ///     Player for which this event fired.
        /// </summary>
        public LavaPlayer Player { get; }

        /// <summary>
        ///     Track sent by Lavalink.
        /// </summary>
        public LavaTrack Track { get; }

        /// <summary>
        ///     Track's current position
        /// </summary>
        public TimeSpan Position { get; }

        internal PlayerUpdateEventArgs(LavaPlayer player, PlayerUpdateResponse response)
        {
            Player = player;
            Track = player.Track;
            Position = response.State.Position;

            player.UpdatePlayer(x =>
            {
                player.Track.TrackPosition = (long) response.State.Position.TotalMilliseconds;
                player.LastUpdate = response.State.Time;
            });
        }
    }
}