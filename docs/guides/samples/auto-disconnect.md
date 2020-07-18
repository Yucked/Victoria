---
uid: Guides.Samples.AutoDisconnect
title: Auto Disconnect
---

# Auto Disconenct on Empty Queue
With the addition of [OnTrackStarted](xref:Victoria.LavaNode`1.OnTrackStarted), you can easily manage auto-disconnect once your queue is finished. This example will utilize `CancellationTokenSource` and `Tasks` to hande our disconnect logic.

By no means this example is fail-proof or *ideal*. You're welcome to write your own disconnect logic.

### Laying the foundation
In your AudioService, declare a new `ConcurrentDictionary` field with `ulong` as key and `CancellationTokenSource` as value. The key will be the voice channel ID.
[!code-csharp[AudioService](../snippets/AudioService.cs?range=15-16,17,20,36)]

### Building upon our foundation.
We will then hook into [OnTrackStarted](xref:Victoria.LavaNode`1.OnTrackStarted) event to handle auto-disconnect cancellation. We want to be able to cancel any disconnect tasks if a new track is added/started.
[!code-csharp[AudioService](../snippets/AudioService.cs?range=48-60)]

### Implementing the heart
Time to implement our disconnect logic.
[!code-csharp[AudioService](../snippets/AudioService.cs?range=83-102)]

### Disconnect is a GO
In our [OnTrackEnded](xref:Victoria.LavaNode`1.OnTrackEnded) event, we want to start auto-disconnect task once our queue is empty.
[!code-csharp[AudioService](../snippets/AudioService.cs?range=61-81)]