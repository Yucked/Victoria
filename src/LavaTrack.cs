using System;
using Victoria.Interfaces;

namespace Victoria;

/// <inheritdoc />
public class LavaTrack : ILavaTrack {
    /// <inheritdoc />
    public string Hash { get; }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public string Title { get; }

    /// <inheritdoc />
    public string Author { get; }

    /// <inheritdoc />
    public string Url { get; }

    /// <inheritdoc />
    public TimeSpan Position { get; }

    /// <inheritdoc />
    public TimeSpan Duration { get; }

    /// <inheritdoc />
    public bool IsSeekable { get; }

    /// <inheritdoc />
    public bool IsLiveStream { get; }

    /// <inheritdoc />
    public string Artwork { get; }

    /// <inheritdoc />
    public string ISRC { get; }
}