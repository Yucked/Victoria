---
uid: Guides.Samples.AutoPlay
title: Auto Play
---

# Automatically Play Next Track!
Auto-Play can be easily achieved using [OnTrackEnded](xref:Victoria.LavaNode`1.OnTrackEnded) event and managing music requests using [DefaultQueue](xref:Victoria.DefaultQueue`1).
[!code-csharp[AudioService](../snippets/AudioService.cs?range=61-68,70-81)]
[!code-csharp[AudioModule](../snippets/AudioModule.cs?range=68-127)]