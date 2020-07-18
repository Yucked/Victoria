---
uid: Guides.Samples.FetchingLyrics
title: Fetching Lyrics
---

# Searching track lyrics
You want to spice up your bot? Say no more fam! Victoria offers 2 built-in lyric providers: OVH and Genius! Lyrics can be searched using the [LyricsResolve][xref:Victoria.Resolvers.LyricsResolver] class. Extension methods are also provided for [LavaTrack](xref:Victoria.LavaTrack): [FetchLyricsFromGeniusAsync](xref:Victoria.VictoriaExtensions.FetchLyricsFromGeniusAsync*) & [FetchLyricsFromOVHAsync](xref:Victoria.VictoriaExtensions.FetchLyricsFromOVHAsync*).

[!code-csharp[AudioModule](../snippets/AudioModule.cs?range=17-18,291-322)]