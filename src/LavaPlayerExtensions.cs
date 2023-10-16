using System;
using System.Threading.Tasks;
using Victoria.Rest.Filters;
using Victoria.Rest.Payloads;

namespace Victoria;

/// <summary>
/// 
/// </summary>
public static class LavaPlayerExtensions {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="lavaTrack"></param>
    /// <param name="noReplace"></param>
    /// <param name="volume"></param>
    /// <param name="shouldPause"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public static async ValueTask PlayAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                     LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                     TLavaTrack lavaTrack,
                                                                     bool noReplace = true,
                                                                     int volume = default,
                                                                     bool shouldPause = false)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            noReplace,
            new UpdatePlayerPayload(
                EncodedTrack: lavaTrack.Hash,
                Volume: volume,
                IsPaused: shouldPause));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="lavaTrack"></param>
    /// <param name="startTime"></param>
    /// <param name="stopTime"></param>
    /// <param name="noReplace"></param>
    /// <param name="volume"></param>
    /// <param name="shouldPause"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public static async ValueTask PlayAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                     LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                     TLavaTrack lavaTrack,
                                                                     TimeSpan startTime,
                                                                     TimeSpan stopTime,
                                                                     bool noReplace = true,
                                                                     int volume = default,
                                                                     bool shouldPause = false)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            noReplace,
            new UpdatePlayerPayload(
                EncodedTrack: lavaTrack.Hash,
                Volume: volume,
                IsPaused: shouldPause,
                Position: startTime.Milliseconds,
                EndTime: stopTime.Milliseconds));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask StopAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                     LavaNode<TLavaPlayer, TLavaTrack> lavaNode)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(
                EncodedTrack: null));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask PauseAsync() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask ResumeAsync() {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skipAfter"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask<(LavaTrack Skipped, LavaTrack Current)> SkipAsync(TimeSpan? skipAfter = default) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seekPosition"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask SeekAsync(TimeSpan seekPosition) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="volume"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask SetVolumeAsync(int volume) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="equalizerBands"></param>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask EqualizeAsync(params EqualizerBand[] equalizerBands) {
        throw new NotImplementedException();
    }
}