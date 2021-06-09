using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Victoria.Interfaces;
using Victoria.Wrappers;

namespace Victoria.DNET {
    public class LavaNode : LavaNode<ILavaPlayer, ILavaTrack> {
        public LavaNode(NodeConfiguration nodeConfiguration, DiscordSocketClient socketClient,
                        ILogger<ILavaNode> logger)
            : base(nodeConfiguration, socketClient, logger) { }

        public LavaNode(NodeConfiguration nodeConfiguration, DiscordShardedClient shardedClient,
                        ILogger<ILavaNode> logger)
            : base(nodeConfiguration, shardedClient, logger) { }

        public LavaNode(NodeConfiguration nodeConfiguration, BaseSocketClient baseClient, ILogger<ILavaNode> logger)
            : base(nodeConfiguration, baseClient, logger) { }
    }

    public class LavaNode<TLavaPlayer, TLavaTrack> : AbstractLavaNode<TLavaPlayer, TLavaTrack>
        where TLavaPlayer : ILavaPlayer<TLavaTrack>
        where TLavaTrack : ILavaTrack {
        public LavaNode(NodeConfiguration nodeConfiguration, DiscordSocketClient socketClient,
                        ILogger<AbstractLavaNode> logger)
            : this(nodeConfiguration, socketClient as BaseSocketClient, logger) { }

        public LavaNode(NodeConfiguration nodeConfiguration, DiscordShardedClient shardedClient,
                        ILogger<AbstractLavaNode> logger)
            : this(nodeConfiguration, shardedClient as BaseSocketClient, logger) { }

        public LavaNode(NodeConfiguration nodeConfiguration, BaseSocketClient baseClient,
                        ILogger<ILavaNode> logger)
            : base(nodeConfiguration, logger) {
            DiscordClient = new DiscordClient {
                UserId = baseClient.CurrentUser.Id,
                Shards = baseClient switch {
                    DiscordSocketClient                => 0,
                    DiscordShardedClient shardedClient => shardedClient.Shards.Count,
                    _                                  => 0
                }
            };

            baseClient.VoiceServerUpdated += OnVoiceServerUpdated;
            baseClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        }

        public async ValueTask<TLavaPlayer> JoinAsync(IVoiceChannel voiceChannel) {
            if (voiceChannel == null) {
                throw new ArgumentNullException(nameof(voiceChannel));
            }

            if (PlayersCache.TryGetValue(voiceChannel.Id, out var player)) {
                return player;
            }

            await voiceChannel.ConnectAsync();
            player = (TLavaPlayer) Activator.CreateInstance(typeof(TLavaPlayer), WebSocketClient, voiceChannel.Id);
            PlayersCache.TryAdd(voiceChannel.Id, player);

            return player;
        }

        public async ValueTask<TLavaPlayer> LeaveAsync(IVoiceChannel voiceChannel) {
            if (!Volatile.Read(ref IsConnectedRef)) {
                throw new InvalidOperationException("Cannot execute this action when no connection is established");
            }

            if (!PlayersCache.TryRemove(voiceChannel.Id, out var player)) {
                throw new InvalidOperationException("");
            }

            await player.DisposeAsync();
            return player;
        }

        public async ValueTask MoveAsync() {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="voiceChannel"></param>
        /// <returns></returns>
        public bool HasPlayer(IVoiceChannel voiceChannel) {
            return HasPlayer(voiceChannel.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="voiceChannel"></param>
        /// <param name="lavaPlayer"></param>
        /// <returns></returns>
        public bool TryGetPlayer(IVoiceChannel voiceChannel, out TLavaPlayer lavaPlayer) {
            return TryGetPlayer(voiceChannel.Id, out lavaPlayer);
        }

        private Task OnVoiceServerUpdated(SocketVoiceServer voiceServer) {
            return DiscordClient.OnVoiceServerUpdated.Invoke(new VoiceServer {
                GuildId = voiceServer.Guild.Id,
                Endpoint = voiceServer.Endpoint,
                Token = voiceServer.Token
            }).AsTask();
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState,
                                             SocketVoiceState currentState) {
            return DiscordClient.OnUserVoiceStateUpdated.Invoke(new VoiceState {
                UserId = user.Id,
                SessionId = currentState.VoiceSessionId ?? oldState.VoiceSessionId,
                GuildId = (currentState.VoiceChannel ?? oldState.VoiceChannel).Guild.Id,
                ChannelId = (currentState.VoiceChannel ?? oldState.VoiceChannel).Id
            }).AsTask();
        }
    }
}