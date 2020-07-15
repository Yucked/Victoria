---
uid: Guides.GettingStarted.QuickStart
title: ⚗️ Quick Start
---

# ⚗️ Quick Start
Getting started with Victoria is fairly simple and quick:
- Add Victoria package from Nuget.
- Add `LavaNode` and `LavaConfig` to `ServiceCollection`.
```cs
	var services = new ServiceCollection()
		// Other services DiscordSocketClient, CommandService, etc
		.AddSingleton<LavaNode>()
		.AddSingleton<LavaConfig>();
		
	var provider = services.BuildServiceProvider();
```

> [!WARNING]
> Make sure there is **__ONLY ONE__** instance of LavaNode and LavaConfig in your program unless you have several Lavalink instances running and would like to create node pool (which majority of the users don't).

- In your `DiscordSocketClient` or `DiscordShardedClient` `Ready` event call `_instanceOfLavaNode.ConnectAsync();`
```cs
	discordSocketClient.Ready += OnReadyAsync;
	....
	
	private async Task OnReadyAsync() {
		if (!_instanceOfLavaNode.IsConnected) {
			_instanceOfLavaNode.ConnectAsync();
		}
		
		// Other ready related stuff
	}
```

> [!WARNING]
> Avoid calling ConnectAsync multiple times if LavaNode is already connected otherwise an `InvalidOperationException` is thrown.

- Create a Music/Audio command module and start writing Victoria related commands!
[!code-csharp[AudioModule](../snippets/AudioModule.cs?range=5-16,18-45)]