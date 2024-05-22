<p align="center">
	<img src="https://i.imgur.com/Iv7AW9g.png" />
	</br>
	<a href="https://discord.gg/ZJaVXK8">
		<img src="https://img.shields.io/badge/Discord-Support-%237289DA.svg?logo=discord&style=for-the-badge&logoWidth=30&labelColor=0d0d0d" />
	</a>
	<a href="https://www.nuget.org/packages/Victoria/">
		<img src="https://img.shields.io/nuget/dt/Victoria.svg?label=Downloads&logo=nuget&style=for-the-badge&logoWidth=30&labelColor=0d0d0d" />
	</a>
	<p align="center">
	     🌋 - Lavalink wrapper for Discord.NET which provides more options and performs better than all .NET Lavalink libraries combined.
  </p>
</p>


### `Changelog v7.0`
Victoria 7.x rewrites focuses on Lavalink v4 support. Lavalink v4 introduces several changes which break Victoria's functionality on v6.x, to avoid codebase overhaul on user's end, breaking changes are minimal on Victoria' end.

#### `➕ Additions:`
- `ExceptionSeverity` for Lavalink exceptions
- Introduced `ISRC`, `SourceName`, `PluginInfo`, `Artwork` properties in `LavaTrack`
- Added `LavaNodeExtensions`, `LavaPlayerExtensions` for non-API methods.
- Added `GetLavalinkVersionAsync`, `GetLavalinkStatsAsync`, `UpdatePlayerAsync`, `UpdateSessionAsync`, `DestroyPlayerAsync` methods to `LavaNode`.

#### `❌ Removals:`
- `ArtworkResolver`, `LyricsResolver` have been moved to Victoria.Addons, this will be a separate package.
- Removed `Bands`, `TextChannel`, `VoiceChannel`, `IsConnected` properties from `LavaPlayer`.

#### `💀 Modifications:`
- `Vueue` is renamed to `LavaQueue`, this change my be reverted.
- `LavaPlayer` no longer contains any methods for controlling the player.
- `RoutePlanner` has been merged with `LavaNode`.

### `Quick Start`

Full example code @ https://github.com/Yucked/Victoria/tree/examples/v7

> 🐲 Program.cs
```cs
            var serviceCollection = new ServiceCollection()
                // .. other services
                .AddLogging(x => {
                    x.ClearProviders();
                    x.AddColorfulConsole();
                    x.SetMinimumLevel(LogLevel.Trace);
                })
                .AddLavaNode()                
                .AddSingleton<AudioService>();
```

> 🤖 Discord Service / Events Handler
```cs
        private async Task OnReadyAsync() {
            // connect to lavalink
            await _serviceProvider.UseLavaNodeAsync();
        }
```
> 🎸 AudioModule.cs
```cs
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
```
