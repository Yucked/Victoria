<p align="center">
	<img src="https://i.imgur.com/OibdkEz.png" />
	</br>
	<a href="https://discord.gg/ZJaVXK8">
		<img src="https://img.shields.io/badge/Discord-Support-%237289DA.svg?logo=discord&style=for-the-badge&logoWidth=30&labelColor=0d0d0d" />
	</a>
	<a href="https://github.com/Yucked/Victoria/actions">
		<img src="https://img.shields.io/github/workflow/status/Yucked/Victoria/.NET%20Core%20Workflow?label=BUILD%20STATUS&logo=github&style=for-the-badge&logoWidth=30&color=181717" />
	</a>
	<a href="https://www.nuget.org/packages/Victoria/">
		<img src="https://img.shields.io/nuget/dt/Victoria.svg?label=Downloads&logo=nuget&style=for-the-badge&logoWidth=30&labelColor=0d0d0d" />
	</a>
	<a href="">
	    <img src="https://img.shields.io/badge/Yucked*stellarport.io-9003fc?logoColor=white&logo=stellar&style=for-the-badge&&logoWidth=30" />
	</a>
	<p align="center">
	     🌋 - Lavalink wrapper for Discord.NET which provides more options and performs better than all .NET Lavalink libraries combined.
  </p>
</p>

---

## `⚔️ Features:`

With the release of Version 5, Victoria has now features from all previous versions. Some of these features are:

- Keeping up to date with Lavalink features
- Ability to extend Victoria for your needs
- Lyrics support from OVH and Genius
- Artwork support for YouTube, Vimeo, SoundCloud, Twitch
- Built-in Queue support, built on top of `LinkedList`
- Completely asynchronous
- Fast deserialization and serialization with STJ and custom converters
- Decoding track string with supernova speed (Thanks to Pluspy!)
- Easy to understand API with complete documentation
- AND a loving community without whom this project wouldn't be possible!

> #### 👉 Please read the release notes to see what got changed, removed and, modified!

## `⚗️ Quick Start:`

Getting started with Victoria is fairly simple and quick:

- Add Victoria package from Nuget.
- Add `LavaNode` and `LavaConfig` to `ServiceCollection`.

```cs
// Make sure there is ONLY ONE instance of LavaNode and LavaConfig in your program unless you have several
// Lavalink instances running and would like to create node pool (which majority of the users don't).
// For version 5.1.2 and before.

	var services = new ServiceCollection()
		// Other services DiscordSocketClient, CommandService, etc
		.AddSingleton<LavaNode>()
		.AddSingleton<LavaConfig>();
		
	var provider = services.BuildServiceProvider();
```

```cs
// Make sure there is ONLY ONE instance of LavaNode and LavaConfig in your program unless you have several
// Lavalink instances running and would like to create node pool (which majority of the users don't).
// For versions 5.1.3 and above.

	var services = new ServiceCollection()
		// Other services DiscordSocketClient, CommandService, etc
		.AddLavaNode(x => {
            x.SelfDeaf = false;
        });
		
	var provider = services.BuildServiceProvider();
```

- In your `DiscordSocketClient` or `DiscordShardedClient` `Ready` event call `_instanceOfLavaNode.ConnectAsync();`

```cs
	discordSocketClient.Ready += OnReadyAsync;
	....
	
	private async Task OnReadyAsync() {
	// Avoid calling ConnectAsync again if it's already connected 
	// (It throws InvalidOperationException if it's already connected).
		if (!_instanceOfLavaNode.IsConnected) {
			_instanceOfLavaNode.ConnectAsync();
		}
		
		// Other ready related stuff
	}
```

- Create a Music/Audio command module and start writing Victoria related commands!

```cs
public sealed class MusicModule : SocketCommandContext {
	private readonly LavaNode _lavaNode;
	
	public MusicModule(LavaNode lavaNode)
		=> _lavaNode = lavaNode;
		
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
}
```

> #### 👉 If you'd like a complete example, head over to https://victoria.yucked.wtf/
