using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GenIVIV.Attributes;
using Microsoft.Extensions.Logging;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;

namespace GenIVIV.Services {
    public sealed class AudioService {
        private readonly LavaNode _lavaNode;
        private readonly ILogger _logger;
        public readonly HashSet<ulong> VoteQueue;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;

        public AudioService(LavaNode lavaNode, ILoggerFactory loggerFactory) {
            _lavaNode = lavaNode;
            _logger = loggerFactory.CreateLogger<LavaNode>();
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

            VoteQueue = new HashSet<ulong>();

            _lavaNode.OnTrackEnd += OnTrackEndAsync;
            _lavaNode.OnTrackStart += OnTrackStartAsync;
            _lavaNode.OnStatsReceived += OnStatsReceivedAsync;
            _lavaNode.OnUpdateReceived += OnUpdateReceivedAsync;
            _lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;
            _lavaNode.OnTrackStuck += OnTrackStuckAsync;
            _lavaNode.OnTrackException += OnTrackExceptionAsync;
        }

        private static Task OnTrackExceptionAsync(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg) {
            arg.Player.Vueue.Enqueue(arg.Track);
            return arg.Player.TextChannel.SendMessageAsync($"{arg.Track} has been requeued because it threw an exception.");
        }

        private static Task OnTrackStuckAsync(TrackStuckEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg) {
            arg.Player.Vueue.Enqueue(arg.Track);
            return arg.Player.TextChannel.SendMessageAsync($"{arg.Track} has been requeued because it got stuck.");
        }

        private Task OnWebSocketClosedAsync(WebSocketClosedEventArg arg) {
            _logger.LogCritical($"{arg.Code} {arg.Reason}");
            return Task.CompletedTask;
        }

        private Task OnStatsReceivedAsync(StatsEventArg arg) {
            _logger.LogInformation(JsonSerializer.Serialize(arg));
            return Task.CompletedTask;
        }

        private static Task OnUpdateReceivedAsync(UpdateEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg) {
            return arg.Player.TextChannel.SendMessageAsync(
                $"Player update received: {arg.Position}/{arg.Track?.Duration}");
        }

        private static Task OnTrackStartAsync(TrackStartEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg) {
            return arg.Player.TextChannel.SendMessageAsync($"Started playing {arg.Track}.");
        }

        private static Task OnTrackEndAsync(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg) {
            return arg.Player.TextChannel.SendMessageAsync($"Finished playing {arg.Track}.");
        }
    }
}
