using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria.Rest;
using Victoria.Rest.Filters;

namespace Victoria;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TLavaTrack"></typeparam>
public class LavaPlayer<TLavaTrack>
    where TLavaTrack : LavaTrack {
    /// <summary>
    /// 
    /// </summary>
    public ulong GuildId { get; }

    /// <summary>
    /// 
    /// </summary>
    public LavaTrack Track { get; }

    /// <summary>
    /// 
    /// </summary>
    public int Volume { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsPaused { get; }

    /// <summary>
    /// 
    /// </summary>
    public Filters Filters { get; }

    /// <summary>
    /// 
    /// </summary>
    public VoiceState VoiceState { get; }

    /// <summary>
    /// 
    /// </summary>
    public ulong VoiceChannelId { get; }

    /// <summary>
    /// 
    /// </summary>
    public IReadOnlyCollection<EqualizerBand> Bands { get; }

    /// <summary>
    /// 
    /// </summary>
    public LavaQueue<LavaTrack> Queue { get; }

    internal LavaPlayer() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaTrack"></param>
    /// <param name="noReplace"></param>
    /// <param name="volume"></param>
    /// <param name="shouldPause"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask PlayAsync(LavaTrack lavaTrack, bool noReplace = true, int volume = default,
                                     bool shouldPause = false) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaTrack"></param>
    /// <param name="startTime"></param>
    /// <param name="stopTime"></param>
    /// <param name="noReplace"></param>
    /// <param name="volume"></param>
    /// <param name="shouldPause"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask PlayAsync(LavaTrack lavaTrack, TimeSpan startTime, TimeSpan stopTime, bool noReplace = true,
                                     int volume = default, bool shouldPause = false) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask StopAsync() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask PauseAsync() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask ResumeAsync() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skipAfter"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask<(LavaTrack Skipped, LavaTrack Current)> SkipAsync(TimeSpan? skipAfter = default) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seekPosition"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask SeekAsync(TimeSpan seekPosition) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="volume"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask SetVolumeAsync(int volume) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="equalizerBands"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask EqualizeAsync(params EqualizerBand[] equalizerBands) {
        throw new NotImplementedException();
    }
}