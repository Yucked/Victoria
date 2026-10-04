using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Victoria.Rest.Filters;
using Victoria.Rest.Payloads;

namespace Victoria;

/// <summary>
///     Extension methods for controlling a <see cref="LavaPlayer{TLavaTrack}" /> — playback, seeking, volume, and queue management.
/// </summary>
public static class LavaPlayerExtensions {
    internal static ConcurrentDictionary<ulong, LavaQueue<LavaTrack>> Queue { get; } = new();
    
    /// <summary>
    ///     Returns the <see cref="LavaQueue{T}" /> associated with the given player's guild,
    ///     creating an empty queue if one does not yet exist.
    /// </summary>
    /// <param name="player">The player whose guild queue should be retrieved.</param>
    /// <returns>The <see cref="LavaQueue{T}" /> for the player's guild.</returns>
    public static LavaQueue<LavaTrack> GetQueue(this LavaPlayer<LavaTrack> player) {
        if (!Queue.TryGetValue(player.GuildId, out var queue)) {
            queue = new LavaQueue<LavaTrack>();
            Queue.TryAdd(player.GuildId, queue);
        }

        return queue;
    }
    
    /// <summary>
    ///     Starts playing <paramref name="lavaTrack" /> on the player.
    /// </summary>
    /// <param name="lavaPlayer">The player on which to start playback.</param>
    /// <param name="lavaNode">The node used to send the update request.</param>
    /// <param name="lavaTrack">The track to play.</param>
    /// <param name="noReplace">
    ///     When <see langword="true" /> (default), the request is ignored if a track is already playing.
    /// </param>
    /// <param name="volume">Initial playback volume (0–1000). <c>0</c> leaves the current volume unchanged.</param>
    /// <param name="shouldPause">When <see langword="true" />, the player starts in a paused state.</param>
    /// <typeparam name="TLavaPlayer">The concrete player type.</typeparam>
    /// <typeparam name="TLavaTrack">The concrete track type.</typeparam>
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
    ///     Starts playing <paramref name="lavaTrack" /> on the player between the specified time boundaries.
    /// </summary>
    /// <param name="lavaPlayer">The player on which to start playback.</param>
    /// <param name="lavaNode">The node used to send the update request.</param>
    /// <param name="lavaTrack">The track to play.</param>
    /// <param name="startTime">Position in the track at which playback should begin.</param>
    /// <param name="stopTime">Position in the track at which playback should end.</param>
    /// <param name="noReplace">
    ///     When <see langword="true" /> (default), the request is ignored if a track is already playing.
    /// </param>
    /// <param name="volume">Initial playback volume (0–1000). <c>0</c> leaves the current volume unchanged.</param>
    /// <param name="shouldPause">When <see langword="true" />, the player starts in a paused state.</param>
    /// <typeparam name="TLavaPlayer">The concrete player type.</typeparam>
    /// <typeparam name="TLavaTrack">The concrete track type.</typeparam>
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
    ///     Stops playback on the player. Optionally passes a replacement track to avoid a gap in playback.
    /// </summary>
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
                EncodedTrack: lavaTrack?.Hash,
                IsPaused: shouldPause));
    }
    
    /// <summary>
    ///     Pauses the currently playing track without removing it from the player.
    /// </summary>
    /// <param name="lavaPlayer">The player to pause.</param>
    /// <param name="lavaNode">The node used to send the update request.</param>
    /// <typeparam name="TLavaPlayer">The concrete player type.</typeparam>
    /// <typeparam name="TLavaTrack">The concrete track type.</typeparam>
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
    ///     Resumes a paused player, continuing playback of <paramref name="lavaTrack" />.
    /// </summary>
    /// <typeparam name="TLavaPlayer">The concrete player type.</typeparam>
    /// <typeparam name="TLavaTrack">The concrete track type.</typeparam>
    /// <param name="lavaPlayer">The player to resume.</param>
    /// <param name="lavaNode">The node used to send the update request.</param>
    /// <param name="lavaTrack">The track to resume playback for.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous resume operation.</returns>
    public static async ValueTask ResumeAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                       LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                       TLavaTrack lavaTrack)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(
                EncodedTrack: lavaTrack.Hash,
                IsPaused: false));
    }
    
    /// <summary>
    ///     Dequeues the next track and begins playing it, returning both the skipped and the newly playing track.
    /// </summary>
    /// <param name="lavaPlayer">The player on which to skip.</param>
    /// <param name="lavaNode">The node used to send the update request.</param>
    /// <param name="skipAfter">Optional delay before the skip is executed.</param>
    /// <returns>
    ///     A tuple of (<c>Skipped</c>, <c>Current</c>) tracks.
    ///     Returns the default value if there is no queue for the guild.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
    public static async ValueTask<(TLavaTrack Skipped, TLavaTrack Current)> SkipAsync<TLavaPlayer, TLavaTrack>(
        this LavaPlayer<TLavaTrack> lavaPlayer,
        LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
        TimeSpan? skipAfter = default)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        if (!Queue.TryGetValue(lavaPlayer.GuildId, out var queue)) {
            return default;
        }
        
        if (!queue.TryDequeue(out var lavaTrack)) {
            throw new InvalidOperationException("There aren't any more tracks in the Vueue.");
        }
        
        var skippedTrack = lavaPlayer.Track as TLavaTrack;
        await Task.Delay(skipAfter ?? TimeSpan.Zero);
        await PlayAsync(lavaPlayer, lavaNode, (TLavaTrack)lavaTrack);
        
        return (skippedTrack, (TLavaTrack)lavaTrack);
    }
    
    /// <summary>
    ///     Seeks to the specified position in the currently playing track.
    /// </summary>
    /// <param name="lavaPlayer">The player on which to seek.</param>
    /// <param name="lavaNode">The node used to send the update request.</param>
    /// <param name="seekPosition">The position to seek to within the track.</param>
    /// <typeparam name="TLavaPlayer">The concrete player type.</typeparam>
    /// <typeparam name="TLavaTrack">The concrete track type.</typeparam>
    public static async ValueTask SeekAsync<TLavaPlayer, TLavaTrack>(this LavaPlayer<TLavaTrack> lavaPlayer,
                                                                     LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
                                                                     TimeSpan seekPosition)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        await lavaNode.UpdatePlayerAsync(
            lavaPlayer.GuildId,
            updatePayload: new UpdatePlayerPayload(Position: (long)seekPosition.TotalMilliseconds));
    }
    
    /// <summary>
    ///     Sets the playback volume on the player. Valid range is 0–1000; values above 100 amplify audio.
    /// </summary>
    /// <param name="lavaPlayer">The player whose volume should be changed.</param>
    /// <param name="lavaNode">The node used to send the update request.</param>
    /// <param name="volume">The desired volume level (0–1000).</param>
    /// <typeparam name="TLavaPlayer">The concrete player type.</typeparam>
    /// <typeparam name="TLavaTrack">The concrete track type.</typeparam>
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
    ///     Applies equalizer band gains to the player's audio output.
    /// </summary>
    /// <param name="lavaPlayer">The player to equalize.</param>
    /// <param name="lavaNode">The node used to send the update request.</param>
    /// <param name="equalizerBands">One or more <see cref="EqualizerBand" /> values describing the gain adjustments.</param>
    /// <typeparam name="TLavaPlayer">The concrete player type.</typeparam>
    /// <typeparam name="TLavaTrack">The concrete track type.</typeparam>
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