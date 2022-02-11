using System;
using Victoria.Player.Args;

namespace Victoria.Player {
    /// <summary>
    ///     Specifies the reason for why the track ended.
    /// </summary>
    public enum TrackEndReason : byte {
        /// <summary>
        ///     This means that the track itself emitted a terminator. This is usually caused by the track reaching the end,
        ///     however it will also be used when it ends due to an exception.
        /// </summary>
        Finished = (byte) 'F',

        /// <summary>
        ///     This means that the track failed to start, throwing an exception before providing any audio.
        /// </summary>
        LoadFailed = (byte) 'L',

        /// <summary>
        ///     The track was stopped due to the player being stopped by either calling
        ///     <see cref="LavaPlayer.StopAsync" /> or <see cref="LavaPlayer.PlayAsync" />.
        /// </summary>
        Stopped = (byte) 'S',

        /// <summary>
        ///     The track stopped playing because a new track started playing. Note that with this reason, the old track will still
        ///     play until either its buffer runs out or audio from the new track is available.
        /// </summary>
        Replaced = (byte) 'R',

        /// <summary>
        ///     The track was stopped because the cleanup threshold for the audio player was reached. This triggers when the amount
        ///     of time passed since the last call to AudioPlayer#provide() has reached the threshold specified in player manager
        ///     configuration. This may also indicate either a leaked audio player which was discarded, but not stopped.
        /// </summary>
        Cleanup = (byte) 'C'
    }
}