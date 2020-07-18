---
uid: troubleshoot.md
title: Troubleshooting Errors
---

# Troubleshooting Errors
Troubleshooting is dreaded by us all. Some flock to GitHub/Stackoverflow to fix it themselves and other simply open issues. Whatever route you decided to pick, please provide as much information as possible when describing your issue.

---

## 🌋 Lavalink
Often times problem isn't related to Victoria but Lavalink instead.  It is crucial that you monitor Lavalink logs along with Victoria's to find the cause of the problem. Below are some of the most frequent errors that happen:

### Failed Connection
```
java.lang.IllegalStateException: Failed to connect to wss://eu-west440.discord.media/?v=4
        at space.npstr.magma.impl.connections.hax.ClosingUndertowWebSocketClient$1.handleFailed(ClosingUndertowWebSocketClient.java:79) ~[impl-0.12.5.jar!/:na]
```

Lavalink server suggests downgrading/upgrading to Java 12/13. Java 14 and 11 has websocket related issues. Or, try changing your server region(?).

### Playback Error
```
ERROR 104056 [back 1 thread I] c.s.d.l.t.p.LocalAudioTrackExecutor: 
    Error in playback of TRACK_ID
com.sedmellug.discord.lavaplayer.tools.FriendlyException: 
    Something broke when playing the track.
```

There errors emerge from Lavalink and aren't related to Victoria. Please take the following steps in order to resolve these errors:

- Check Lavalink's recent CI builds. More often than not these issues are fixed in their CI builds before an official GitHub release is made.

- Try requeue-ing the track. You'd need to use [LavaNode#OnTrackException](xref:Victoria.LavaNode`1.OnTrackException) or [LavaNode#OnTrackStuck](xref:Victoria.LavaNode`1.OnTrackStuck).

[LavaTrack]: xref:Victoria.LavaTrack
[SearchResponse]: xref:Victoria.Responses.Rest.SearchResponse
[SearchResponse#LoadStatus]: xref:Victoria.Enums.LoadStatus
[SearchAsync]: xref:Victoria.LavaNode`1.SearchAsync*
[SearchYouTubeAsync]: xref:Victoria.LavaNode`1.SearchYouTubeAsync*
[SearchSoundCloudAsync]: xref:Victoria.LavaNode`1.SearchSoundCloudAsync*
[SearchResponse#Tracks]: xref:Victoria.Responses.Rest.SearchResponse.Tracks

- Before adding [LavaTrack] to queue, please make sure of the following:
    - Check whether [SearchAsync], [SearchYouTubeAsync], [SearchSoundCloudAsync] return `null`/`default` [SearchResponse].
    - Check whether [SearchResponse#LoadStatus] is `null`, `LoadFailed`, or `NoMatches`.
    - Check whether [SearchResponse#Tracks] is empty or `null`.
    - Check whether `FirstOrDefault` on [SearchResponse#Tracks] returns a `null`/`default` [LavaTrack].