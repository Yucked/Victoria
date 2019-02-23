using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Victoria.Entities;
using Victoria.Entities.Enums;
using Victoria.Entities.Responses;
using Victoria.Helpers;

namespace Victoria
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LavaSocket : IAsyncDisposable
    {
        /// <summary>
        /// Fires when stats are sent from Lavalink server.
        /// </summary>
        public Func<ServerStats, Task> StatsReceived;

        /// <summary>
        /// Fires when a track has timed out.
        /// </summary>
        public Func<LavaPlayer, LavaTrack, long, Task> TrackStuck;

        /// <summary>
        /// Fires when a track throws an exception.
        /// </summary>
        public Func<LavaPlayer, LavaTrack, string, Task> TrackException;

        /// <summary>
        /// Fires when player update is sent from lavalink server.
        /// </summary>
        public Func<LavaPlayer, LavaTrack, TimeSpan, Task> PlayerUpdated;

        /// <summary>
        /// Fires when any of the <see cref="TrackReason"/> 's are met.
        /// </summary>
        public Func<LavaPlayer, LavaTrack, TrackEndReason, Task> TrackFinished;

        private readonly BaseSocketClient _baseSocketClient;
        private SocketVoiceState cachedStated;

        internal LavaSocket(BaseSocketClient baseSocketClient)
        {
            _baseSocketClient = baseSocketClient;
            baseSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            baseSocketClient.VoiceServerUpdated += OnVoiceServerUpdated;

            switch (baseSocketClient)
            {
                case DiscordSocketClient socketClient:
                    break;

                case DiscordShardedClient shardedClient:
                    break;
            }
        }


        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState currentState)
        {
            if (user.Id != _baseSocketClient.CurrentUser.Id)
                return Task.CompletedTask;

            return Task.CompletedTask;
        }

        private Task OnVoiceServerUpdated(SocketVoiceServer server)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}