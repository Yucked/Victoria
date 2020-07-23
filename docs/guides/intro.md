---
uid: Guides.Introduction
title: Introduction
---

# Introduction
Victoria utilizes several advance concepts similar to Discord.NET. The code style is rather easy to follow. Objects, properties and methods follow simple naming conventions. The main objects in Victoria are the following:

[LavaNode]:xref:Victoria.LavaNode`1
- [LavaNode] represents a single connection to a Lavalink server. If you've mulitple Lavalink servers running, you'd need multiple [LavaNode]s for each server. Furthermore, you can use them for load balancing purposes.

- [LavaConfig](xref:Victoria.LavaConfig) is used to configure connection and configuration for each [LavaNode].

[LavaPlayer]:xref:Victoria.LavaPlayer
- [LavaPlayer]  represents a single voice channel connection or guild connection. One guild can only have a single [LavaPlayer].

- [LavaSocket](xref:Victoria.LavaSocket) is a wrapper around ClientWebSocket since ClientWebSocket lacks features such as Auto-Reconnect, Events and so on.

- [DefaultQueue](xref:Victoria.DefaultQueue`1) is based on LinkedList<T>. Why not Queue or ConcurrentQueue? Simply because of limitations and difficulty in terms of extensions.

If you're new to programming in general I suggest you utilize the following resources to familiarize yourself with C# and .NET ecosystem.

- Official C# Documentation: https://docs.microsoft.com/en-us/dotnet/csharp/
- Official .NET Documentation: https://docs.microsoft.com/en-us/dotnet/

Older versions of Victoria have used `.NET Standard` and these versions include: 1.x, 2.x, 3.x. Versions from 4.x and above target `.NET Core` by default.