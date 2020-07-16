---
uid: Guides.Samples.AutoDisconnect
title: Auto Disconnect
---

# Auto Disconenct on Empty Queue
With the addition of [OnTrackStarted](xref:Victoria.LavaNode`1.OnTrackStarted), you can easily manage auto-disconnect once your queue is finished.
[!code-csharp[AudioService](../snippets/AudioService.cs?range=15-16,17,20,36-37,48-60,61-102)]