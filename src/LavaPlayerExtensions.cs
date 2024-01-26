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
                                                                     LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                     TLavaTrack lavaTrack,
                                                                     bool noReplace = false,
                                                                     bool shouldPause = true)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            noReplace,
            updatePayload: new UpdatePlayerPayload(
                EncodedTrack: lavaTrack == null ? null : lavaTrack.Hash,
                IsPaused: shouldPause));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public static async ValueTask PauseAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                      LavaNode<TLavaPlayer, TLavaTrack> lavaNode)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(
                IsPaused: true));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="noReplace"></param>
    /// <returns></returns>
    public static async ValueTask ResumeAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                       LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                       TLavaTrack lavaTrack,
                                                                       bool noReplace = false)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            noReplace,
            updatePayload: new UpdatePlayerPayload(
                EncodedTrack: lavaTrack.Hash,
                IsPaused: false));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skipAfter"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask<(LavaTrack Skipped, LavaTrack Current)> SkipAsync(TimeSpan? skipAfter = default) {
        // TODO: Depends on queue
        return default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="seekPosition"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public static async ValueTask SeekAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                     LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                     TimeSpan seekPosition)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(Position: seekPosition.Milliseconds));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="volume"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    public static async ValueTask SetVolumeAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                          LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                          int volume)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(Volume: volume));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaPlayer"></param>
    /// <param name="lavaNode"></param>
    /// <param name="equalizerBands"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    /// <exception cref="NotImplementedException"></exception>
    public static async ValueTask EqualizeAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                         LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                         params EqualizerBand[] equalizerBands)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(Filters: new Filters {
                Bands = equalizerBands
            }));
    }
}