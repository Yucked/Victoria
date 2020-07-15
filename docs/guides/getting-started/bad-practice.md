---
uid: Guides.GettingStarted.BadPractice
title: ❌ Bad Practice
---

## ❌ Bad Practice
On several occasions I've seen users do the following in their `Program.cs` or `MyBot.cs` when configuring services.

```cs
public class Program {
	// Instance #1
	public static LavaNode LavaNode;
	private DiscordSocketClient socketClient;
	private CommandService commandService;

	public static void Main(string[] args) {
		new Program().RunAsync().GetAwaiter().GetResult();
	}

	public async Task RunAsync() {
		LavaNode = new LavaNode(new LavaConfig());
		socketClient = new DiscordSocketClient();
		commandService = new CommandService();

		var services = new ServiceCollection()
			// Instance #2
			.AddSingleton<LavaNode>()
			.AddSingleton<LavaConfig>()
			.AddSingleton(socketClient)
			.AddSingleton(commandService)
			.BuildServiceProvider();

		socketClient.Ready += ReadyAsync;
		// do other stuff here
	}

	private async Task ReadyAsync() {
		LavaNode.ConnectAsync();
	}
}
```

Then in their `AudioService` or `AudioModule` they use the injected service.
```cs
public class AudioModule : SocketCommandContext {
	private readonly LavaNode _lavaNode;

	public AudioModule(LavaNode lavaNode) {
		_lavaNode = lavaNode;
	}

	// Play, Pause, etc commands.
}

```

And they wonder why Victoria won't work. If you refer to `Warnings` above, you will realize why Victoria isn't working out. \
If you aren't familiar with IOC/DI pattern, please familiarize yourself with said principle as Discord.NET is heavily dependent on them.

> [!NOTE]
> - [Dependency Injection Fundamentals in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1)
> - [What is Inversion of Control (IOC)?](https://stackoverflow.com/questions/3058/what-is-inversion-of-control)