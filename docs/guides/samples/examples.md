---
uid: examples.md
title: Code Examples
---

# Code Examples

Have you thought 🤔 about doing advance stuff with Victoria? 📈 Taking things to the next level? Introduce new Rhythm bot to Discord market?
Well, you can certainly do it with Victoria! This guide covers several advance topics that can make your Discord bot standout.

## Automatically Play Next Track!
Auto-Play can be easily achieved using [OnTrackEnded](xref:Victoria.LavaNode`1.OnTrackEnded) event and managing music requests using [DefaultQueue](xref:Victoria.DefaultQueue`1).
[!code-csharp[AudioService](../snippets/AudioService.cs?range=61-68,70-81)]
[!code-csharp[AudioModule](../snippets/AudioModule.cs?range=68-127)]

---
## Requeue on Track Exception/Stuck
Usually used to tackle Lavalink throwing [Track exceptions](xref:troubleshoot.md#playback-error).
[!code-csharp[AudioService](../snippets/AudioService.cs#L103-L117)]

---
## Auto Disconenct on Empty Queue
With the addition of [OnTrackStarted](xref:Victoria.LavaNode`1.OnTrackStarted), you can easily manage auto-disconnect once your queue is finished.
[!code-csharp[AudioService](../snippets/AudioService.cs?range=15-16,17,20,36-37,48-60,61-102)]