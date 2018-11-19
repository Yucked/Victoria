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
Grab the latest release from [Nuget](https://www.nuget.org/packages/Victoria/). Add `Lavalink` to your `ServiceCollection` or make a global static property of `Lavalink` since it's not a heavy object.

> #### Version 2.x

```cs
// In the ready event of your DiscordSocketClient or DiscordShardedClient add the following code.

private async Task OnReady(){
    // Get Lavalink from service provider
    // LavaConfig is optional
    await Lavalink.ConnectAsync(socketClient, myOptionalLavaConfig).ConfigureAwait(false);
    
    // ConnectAsync returns the node. You can pass that to your Audio Service like so:
    var node = await Lavalink.ConnectAsync(....).ConfigureAwait(false);
    myAudioService.Initialize(node);    
}
```
Then in your `AudioService` or however your bot is structured you can get the DefaultNode or use the node that you passed in `myAudioService.Initialize(node)`.

```cs
public sealed class AudioService {

    private LavaNode _node;
    private Lavalink _lavalink;
    
    public AudioService(Lavalink lavalink)
        => _lavalink = lavalink;
            
    public void Initialize(LavaNode node = null){
            _node = node ?? _lavalink.DefaultNode;
            // You can also get node by using Lavalink.GetNode(EndPoint);
    }
    
    public async Task ConnectAndPlayAsync(IVoiceChannel voiceChannel){
        // JoinAsync returns a LavaPlayer that you can use to play music and stuff.
        var player = await _node.JoinAsync(voiceChannel);
        
        // If you're already connected to a voice channel you can get the existing player via _node.GetPlayer(GUILD_ID);
        var search = await _node.SearchYouTubeAsync(MY_QUERY);
        var track = search.Tracks.FirstOrDefault(); // via Linq.
        player.Play(track);        
    }
}
```

> #### Version 3.x

Since version 3.x is a major rewrite most of the code is somewhat similar and follows the same logic as before except a few things.

```cs
private async Task OnReady(){
    var node = Lavalink.AddNodeAsync(socketClient, myOptionalConfiguration).ConfigureAwait(false); 
    // Pass the above node to your AudioService or call Lavalink.GetNode(MY_NODE_NAME);
}
```

In your AudioService or AudioModule
```cs
var player = await node.ConnectAsync(MY_VOICE_CHANNEL, MY_OPTIONAL_TEXT_CHANNEL);
// You can get an existing player the same way as 2.x

var search = node.GetTracksAsync(MY_QUERY);
var track = search.FirstOrDefault();
await player.PlayAsync(track); 
```

> #### Things to keep in mind:

- Since Lavalink constructor is public in 3.x you can specify a custom prefix for nodes. By default it's `LavaNode_#`
- There needs to be only a **single** instance of Lavalink in both versions.
- As of 3.x there are no `events`. Events have been replaced with `Func<T>` and they are a lot simpler. You just need to match `Func<T>` signature.
All of the `Func<T>` return a Task so for example:
```cs
Lavalink.Log = log => Task.Run(() => Console.WriteLine(log.Message));

node.TrackFinished = OnFinished;

private Task OnFinished(LavaPlayer player, LavaTrack track, TrackReason reason)
    => Task.CompletedTask;
```
- Most of the methods now return a Task and need to be awaited.

## 💡 `I Want X Feature In Victoria!`
You can open an issue and describe your feature with massive details and make sure your feature is required on global scale.

## 🚀 `I Like Victoria! How Can I Support Her?!`
GREAT! SMASH THAT :star: BUTTON, HIT THE :eyes: (watch) BUTTON. Or, you can [Buy Me A Coffee](https://www.buymeacoffee.com/Yucked) for my hardwork.
OR you can spread the word about Victoria. None of them are necessary but it would be greatly appreciated.
