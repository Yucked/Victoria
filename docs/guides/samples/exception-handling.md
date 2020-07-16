---
uid: Guides.Samples.ExceptionHandling
title: Exception Handling
---

# Using Log Event
All of the exceptions are logged to [OnLog](xref:Victoria.LavaNode`1.OnLog) event. In order to catch exceptions and debug, you must subscribe to the event.
[!code-csharp[AudioService](../snippets/AudioService.cs#L22-L25)]

# Handling Track Stuck/Exception Events
Usually used to tackle Lavalink throwing [track exceptions](xref:troubleshoot.md#playback-error).
[!code-csharp[AudioService](../snippets/AudioService.cs#L103-L117)]

# Try/Catch
In v5, throwing exceptions became a common practice instead of using `TryXyz` format where a `bool` would be returned. it is essential that you wrap methods in `try/catch`. A typical example would be:
[!code-csharp[AudioModule](../snippets/AudioModule.cs#L24-L44)]