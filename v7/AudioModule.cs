using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GenIVIV.Services;
using Victoria;
using Victoria.Rest.Search;

namespace GenIVIV.Modules;

public sealed class AudioModule(
    LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode,
    AudioService audioService)
    : ModuleBase<SocketCommandContext> {
    private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);
    
    [Command("Join")]
    public async Task JoinAsync() {
        var voiceState = Context.User as IVoiceState;
        if (voiceState?.VoiceChannel == null) {
            await ReplyAsync("You must be connected to a voice channel!");
            return;
        }
        
        try {
            await lavaNode.JoinAsync(voiceState.VoiceChannel);
            await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            
            audioService.TextChannels.TryAdd(Context.Guild.Id, Context.Channel.Id);
        }
        catch (Exception exception) {
            await ReplyAsync(exception.ToString());
        }
    }
    
    [Command("Leave")]
    public async Task LeaveAsync() {
        var voiceChannel = (Context.User as IVoiceState).VoiceChannel;
        if (voiceChannel == null) {
            await ReplyAsync("Not sure which voice channel to disconnect from.");
            return;
        }
        
        try {
            await lavaNode.LeaveAsync(voiceChannel);
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
        
        var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
        if (player == null) {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }
            
            try {
                player = await lavaNode.JoinAsync(voiceState.VoiceChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
                audioService.TextChannels.TryAdd(Context.Guild.Id, Context.Channel.Id);
            }
            catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }
        
        var searchResponse = await lavaNode.LoadTrackAsync(searchQuery);
        if (searchResponse.Type is SearchType.Empty or SearchType.Error) {
            await ReplyAsync($"I wasn't able to find anything for `{searchQuery}`.");
            return;
        }
        
        var track = searchResponse.Tracks.FirstOrDefault();
        if (player.GetQueue().Count == 0) {
            await player.PlayAsync(lavaNode, track);
            await ReplyAsync($"Now playing: {track.Title}");
            return;
        }
        
        player.GetQueue().Enqueue(track);
        await ReplyAsync($"Added {track.Title} to queue.");
    }
    
    [Command("Pause"), RequirePlayer]
    public async Task PauseAsync() {
        var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
        if (player.IsPaused && player.Track != null) {
            await ReplyAsync("I cannot pause when I'm not playing anything!");
            return;
        }
        
        try {
            await player.PauseAsync(lavaNode);
            await ReplyAsync($"Paused: {player.Track.Title}");
        }
        catch (Exception exception) {
            await ReplyAsync(exception.Message);
        }
    }
    
    [Command("Resume"), RequirePlayer]
    public async Task ResumeAsync() {
        var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
        if (!player.IsPaused && player.Track != null) {
            await ReplyAsync("I cannot resume when I'm not playing anything!");
            return;
        }
        
        try {
            await player.ResumeAsync(lavaNode, player.Track);
            await ReplyAsync($"Resumed: {player.Track.Title}");
        }
        catch (Exception exception) {
            await ReplyAsync(exception.Message);
        }
    }
    
    [Command("Stop"), RequirePlayer]
    public async Task StopAsync() {
        var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
        if (!player.State.IsConnected || player.Track == null) {
            await ReplyAsync("Woah, can't stop won't stop.");
            return;
        }
        
        try {
            await player.StopAsync(lavaNode, player.Track);
            await ReplyAsync("No longer playing anything.");
        }
        catch (Exception exception) {
            await ReplyAsync(exception.Message);
        }
    }
    
    [Command("Skip"), RequirePlayer]
    public async Task SkipAsync() {
        var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
        if (!player.State.IsConnected) {
            await ReplyAsync("Woaaah there, I can't skip when nothing is playing.");
            return;
        }
        
        var voiceChannelUsers = Context.Guild.CurrentUser.VoiceChannel
            .Users
            .Where(x => !x.IsBot)
            .ToArray();
        
        if (!audioService.VoteQueue.Add(Context.User.Id)) {
            await ReplyAsync("You can't vote again.");
            return;
        }
        
        var percentage = audioService.VoteQueue.Count / voiceChannelUsers.Length * 100;
        if (percentage < 85) {
            await ReplyAsync("You need more than 85% votes to skip this song.");
            return;
        }
        
        try {
            var (skipped, currenTrack) = await player.SkipAsync(lavaNode);
            await ReplyAsync($"Skipped: {skipped.Title}\nNow Playing: {currenTrack.Title}");
        }
        catch (Exception exception) {
            await ReplyAsync(exception.Message);
        }
    }
}
