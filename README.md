<p align="center">
  <img src="https://cdn.discordapp.com/attachments/491355652191682582/491355700589756426/la-kru-portrait1104color.png" width="25%"/>
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
 // Get Node
 var node = Lavalink.GetNode(new Endpoint {
   Port = 80,
   Host = "127.0.0.1"
 });
 
 // Join 
var player = await node.JoinAsync(VOICE_CHANNEL);
player.Queue.TryAdd(guildId, new LinkedList<LavaTrack>());
 ....
 
 // Existing
 var player = node.GetPlayer(GUILD_ID);
 
 
 var search = await LavaNode.SearchYouTubeAsync(query);
 var track = search.Tracks.FirstOrDefault();
 
 player.Play(track);
 ```
 
 **Full Example:** https://github.com/Yucked/Bot.NET/tree/master/Galaxy
