<p align="center">
  <img src="https://scontent-ort2-2.cdninstagram.com/vp/1557da3249dfe1b5bba30b70e6272201/5C45E12A/t51.2885-15/e35/41401088_300535220537995_8483483865107589107_n.jpg" width="25%"/>
  <h2> V I C T O R I A </h2>
  </p>
  
 ### `About`
 Victoria is a .NET wrapper for [Lavalink]("").
 
[![NuGet](https://img.shields.io/nuget/v/Nuget.Core.svg?style=for-the-badge&colorA=303030&colorB=f44268&label=NUGET:+Victoria)](https://www.nuget.org/packages/Victoria/)


 > *But we already have Sharplink and Lavalink.NET!!* 
 
 S*#& THE F@$! UP. I did it for the :stars: 🤘
 
 > *What makes it better than Sharplink or Lavalink.Net?*
 
 I mashed up [Emzi's Wrapper]("https://github.com/DSharpPlus/DSharpPlus/tree/master/DSharpPlus.Lavalink") and Sharplink. So, you get good quality code and also [verinaz](https://i.imgur.com/VeJGAi8.gif) performance. ~~I haven't benchmarked it but trust me.~~ 
 
 
 ### `Example`
 
 
 - Add `Lavalink` in `ServiceCollection`.
 ```cs
 var client = new DiscordSocketClient();
 ...
 .AddSingleton<Lavalink>()
 ....
 ```
 
 - Put this in your ready event.
 ```cs
 client.Ready += OnReady;
 
 ...
 
 private async Task OnReady() {
  var node = await Lavalink.ConnectAsync(Client, new LavaConfig {
   MaxTries = 5,
    Authorization = "foo",
    Rest = new Endpoint {
     Port = 2333,
      Host = "127.0.0.1"
    },
    Socket = new Endpoint {
     Port = 80,
      Host = "127.0.0.1"
    }
  });
  AudioService.Initialize(node);
 }
 ```
 
 - Get the LavaNode & Connect to VoiceChannel Or Get an Existing LavaPlayer And Play Some Track.
 ```cs
 // Get Node by Endpoint or get the first node.
 var node = Lavalink.GetNode(new Endpoint {
   Port = 80,
   Host = "127.0.0.1"
 }) ?? Lavalink.DefaultNode;
 
 // Join 
var player = await node.JoinAsync(VOICE_CHANNEL);

// Use build in queue or make your own (No Support)
player.Queue.TryAdd(GUILD_ID, new LinkedList<LavaTrack>());
 ....
 
 // Existing
 var player = node.GetPlayer(GUILD_ID);
 
 
 var search = await node.SearchYouTubeAsync(QUERY);
 var track = search.Tracks.FirstOrDefault();
 
 player.Play(track);
 ```
