using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria.Interfaces;
using Victoria.Rest;
using Victoria.Rest.Filters;

namespace Victoria;

/// <inheritdoc />
public class LavaPlayer : ILavaPlayer<LavaTrack> {
    /// <inheritdoc />
    public ulong GuildId { get; }

    /// <inheritdoc />
    public LavaTrack Track { get; }

    /// <inheritdoc />
    public int Volume { get; }

    /// <inheritdoc />
    public bool IsPaused { get; }

    /// <inheritdoc />
    public IFilters Filters { get; }

    /// <inheritdoc />
    public VoiceState VoiceState { get; }

    /// <inheritdoc />
    public ulong VoiceChannelId { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<EqualizerBand> Bands { get; }

    /// <inheritdoc />
    public LavaQueue<LavaTrack> Queue { get; }

    /// <inheritdoc />
    public async ValueTask PlayAsync(LavaTrack lavaTrack, bool noReplace = true, int volume = default,
                                     bool shouldPause = false) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async ValueTask PlayAsync(LavaTrack lavaTrack, TimeSpan startTime, TimeSpan stopTime, bool noReplace = true,
                                     int volume = default, bool shouldPause = false) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async ValueTask StopAsync() {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async ValueTask PauseAsync() {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async ValueTask ResumeAsync() {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async ValueTask<(LavaTrack Skipped, LavaTrack Current)> SkipAsync(TimeSpan? skipAfter = default) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async ValueTask SeekAsync(TimeSpan seekPosition) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async ValueTask SetVolumeAsync(int volume) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async ValueTask EqualizeAsync(params EqualizerBand[] equalizerBands) {
        throw new NotImplementedException();
    }
}