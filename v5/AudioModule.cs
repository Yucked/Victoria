using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace Example.Modules {
    public sealed class AudioModule : ModuleBase<SocketCommandContext> {
        private readonly LavaNode<XLavaPlayer> _lavaNode;
        private readonly AudioService _audioService;
        private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

        public AudioModule(LavaNode<XLavaPlayer> lavaNode, AudioService audioService) {
            _lavaNode = lavaNode;
            _audioService = audioService;
        }

        [Command("Join")]
        public async Task JoinAsync() {
            if (_lavaNode.HasPlayer(Context.Guild)) {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Leave")]
        public async Task LeaveAsync() {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                await ReplyAsync("I'm not connected to any voice channels!");
                return;
            }

            var voiceChannel = (Context.User as IVoiceState)?.VoiceChannel ?? player.VoiceChannel;
            if (voiceChannel == null) {
                await ReplyAsync("Not sure which voice channel to disconnect from.");
                return;
            }

            try {
                await _lavaNode.LeaveAsync(voiceChannel);
                await ReplyAsync($"I've left {voiceChannel.Name}!");
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Play")]
        public async Task PlayAsync([Remainder] string searchQuery) {
            if (string.IsNullOrWhiteSpace(searchQuery)) {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            var searchResponse = await _lavaNode.SearchAsync(SearchType.Direct, searchQuery);
            if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches) {
                await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name)) {
                player.Queue.Enqueue(searchResponse.Tracks);
                await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} songs.");
            }
            else {
                var track = searchResponse.Tracks.FirstOrDefault();
                player.Queue.Enqueue(track);

                await ReplyAsync($"Enqueued {track?.Title}");
            }

            if (player.PlayerState is PlayerState.Playing or PlayerState.Paused) {
                return;
            }

            player.Queue.TryDequeue(out var lavaTrack);
            await player.PlayAsync(x => {
                x.Track = lavaTrack;
                x.ShouldPause = false;
            });
        }

        [Command("Pause")]
        public async Task PauseAsync() {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing) {
                await ReplyAsync("I cannot pause when I'm not playing anything!");
                return;
            }

            try {
                await player.PauseAsync();
                await ReplyAsync($"Paused: {player.Track.Title}");
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Resume")]
        public async Task ResumeAsync() {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Paused) {
                await ReplyAsync("I cannot resume when I'm not playing anything!");
                return;
            }

            try {
                await player.ResumeAsync();
                await ReplyAsync($"Resumed: {player.Track.Title}");
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Stop")]
        public async Task StopAsync() {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState == PlayerState.Stopped) {
                await ReplyAsync("Woaaah there, I can't stop the stopped forced.");
                return;
            }

            try {
                await player.StopAsync();
                await ReplyAsync("No longer playing anything.");
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Skip")]
        public async Task SkipAsync() {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing) {
                await ReplyAsync("Woaaah there, I can't skip when nothing is playing.");
                return;
            }

            var voiceChannelUsers = (player.VoiceChannel as SocketVoiceChannel)?.Users
                .Where(x => !x.IsBot)
                .ToArray();

            if (_audioService.VoteQueue.Contains(Context.User.Id)) {
                await ReplyAsync("You can't vote again.");
                return;
            }

            _audioService.VoteQueue.Add(Context.User.Id);
            if (voiceChannelUsers != null) {
                var percentage = _audioService.VoteQueue.Count / voiceChannelUsers.Length * 100;
                if (percentage < 85) {
                    await ReplyAsync("You need more than 85% votes to skip this song.");
                    return;
                }
            }

            try {
                var (oldTrack, currenTrack) = await player.SkipAsync();
                await ReplyAsync($"Skipped: {oldTrack.Title}\nNow Playing: {player.Track.Title}");
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }

            _audioService.VoteQueue.Clear();
        }

        [Command("Seek")]
        public async Task SeekAsync(TimeSpan timeSpan) {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing) {
                await ReplyAsync("Woaaah there, I can't seek when nothing is playing.");
                return;
            }

            try {
                await player.SeekAsync(timeSpan);
                await ReplyAsync($"I've seeked `{player.Track.Title}` to {timeSpan}.");
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("Volume")]
        public async Task VolumeAsync(ushort volume) {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            try {
                await player.UpdateVolumeAsync(volume);
                await ReplyAsync($"I've changed the player volume to {volume}.");
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("NowPlaying"), Alias("Np")]
        public async Task NowPlayingAsync() {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing) {
                await ReplyAsync("Woaaah there, I'm not playing any tracks.");
                return;
            }

            var track = player.Track;
            var artwork = await track.FetchArtworkAsync();

            var embed = new EmbedBuilder()
                .WithAuthor(track.Author, Context.Client.CurrentUser.GetAvatarUrl(), track.Url)
                .WithTitle($"Now Playing: {track.Title}")
                .WithImageUrl(artwork)
                .WithFooter($"{track.Position}/{track.Duration}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("Genius", RunMode = RunMode.Async)]
        public async Task ShowGeniusLyrics() {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing) {
                await ReplyAsync("Woaaah there, I'm not playing any tracks.");
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
            if (string.IsNullOrWhiteSpace(lyrics)) {
                await ReplyAsync($"No lyrics found for {player.Track.Title}");
                return;
            }

            await SendLyricsAsync(lyrics);
        }

        [Command("OVH", RunMode = RunMode.Async)]
        public async Task ShowOvhLyrics() {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing) {
                await ReplyAsync("Woaaah there, I'm not playing any tracks.");
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromOvhAsync();
            if (string.IsNullOrWhiteSpace(lyrics)) {
                await ReplyAsync($"No lyrics found for {player.Track.Title}");
                return;
            }

            await SendLyricsAsync(lyrics);
        }

        [Command("Queue")]
        public Task QueueAsync() {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player)) {
                return ReplyAsync("I'm not connected to a voice channel.");
            }

            return ReplyAsync(player.PlayerState != PlayerState.Playing
                ? "Woaaah there, I'm not playing any tracks."
                : string.Join(Environment.NewLine, player.Queue.Select(x => x.Title)));
        }

        private async Task SendLyricsAsync(string lyrics) {
            var splitLyrics = lyrics.Split(Environment.NewLine);
            var stringBuilder = new StringBuilder();
            foreach (var line in splitLyrics) {
                if (line.Contains('[')) {
                    stringBuilder.Append(Environment.NewLine);
                }

                if (Range.Contains(stringBuilder.Length)) {
                    await ReplyAsync($"```{stringBuilder}```");
                    stringBuilder.Clear();
                }
                else {
                    stringBuilder.AppendLine(line);
                }
            }

            await ReplyAsync($"```{stringBuilder}```");
        }
    }
}