<p align="center">
  <img src="https://i.imgur.com/i6wyG8k.gif" widht="70%">
</p>  

Lavalink wrapper for Discord.NET that aims to be better than Sharplink and Lavalink.NET combined.

---


## 🔧`What Is It?`
Victoria is a Lavalink wrapper for Discord.NET library. It uses Emzi's code style while keeping it simple like sharplink.
Even though Sharplink is great, there were constant internal exceptions and weird code style. Victoria aims to solve that and also provide full support of Lavalink.

## 🤔 `How To Use It?`
Make sure you've latest version of Java installed (10 /11) and follow Lavalink [instructions](https://github.com/Frederikam/Lavalink#server-configuration).
Grab the latest release from [Nuget](https://www.nuget.org/packages/Victoria/). Add `Lavalink` to your `ServiceCollection` or make a global static property of `Lavalink` since it's not a heavy object. From there on, in your `DiscordSocketClient`'s or `DiscordShardedClient`'s ready event add

```cs
// Get Lavalink from DI or use your global property. 
// LavaConfig is optional, it will use default Application.yml settings.
  var node = await Lavalink.ConnectAsync(Client, new LavaConfig {
   MaxTries = 5,
    Authorization = "foo",
    Endpoint = new Endpoint {
     Port = 2333,
      Host = "127.0.0.1"
    }
  });
  AudioService.Initialize(node); // Your AudioService.
  ```
  
- Get the `LavaNode` or use `DefaultNode` to join a voice channel and play a track.
 ```cs
 // Get Node by Endpoint or get the first node.
 var node = Lavalink.GetNode(new Endpoint {
  Port = 80,
   Host = "127.0.0.1"
 }) ? ? Lavalink.DefaultNode;

 // Join a voice channel.
 var player = await node.JoinAsync(VOICE_CHANNEL, TEXT_CHANNEL);

 // Or get an existing player.
 var player = node.GetPlayer(GUILD_ID);

 var search = await node.SearchYouTubeAsync(QUERY);
 var track = search.Tracks.FirstOrDefault();

 player.Play(track);
 ```
 
 For full usage please look at this example: [Audio Service & Audio Module](https://github.com/Yucked/Veronica/tree/6e472c53e67514c8dc6d0e51faae7f357f1fbc1e)

## 💡 `I Want X Feature In Victoria!`
You can open an issue and describe your feature with massive details and make sure your feature is required on global scale.

## 🚀 `I Like Victoria! How Can I Support Her?!`
GREAT! SMASH THAT :star: BUTTON, HIT THE :eyes: (watch) BUTTON. Or, you can [Buy Me A Coffee](https://www.buymeacoffee.com/Yucked) for my hardwork.
OR you can spread the word about Victoria. None of them are necessary but it would be greatly appreciated.
