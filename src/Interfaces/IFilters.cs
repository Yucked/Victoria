using System.Collections.Generic;
using Victoria.Rest.Filters;

namespace Victoria.Interfaces;

/// <summary>
/// 
/// </summary>
public interface IFilters {
    /// <summary>
    /// 
    /// </summary>
    float Volume { get; }

    /// <summary>
    /// 
    /// </summary>
    IEnumerable<EqualizerBand> Bands { get; }

    /// <summary>
    /// 
    /// </summary>
    KarokeFilter Karoke { get; }

    /// <summary>
    /// 
    /// </summary>
    TimescaleFilter Timescale { get; }

    /// <summary>
    /// 
    /// </summary>
    TremoloFilter Tremolo { get; }

    /// <summary>
    /// 
    /// </summary>
    VibratoFilter Vibrato { get; }

    /// <summary>
    /// 
    /// </summary>
    RotationFilter Rotation { get; }

    /// <summary>
    /// 
    /// </summary>
    DistortionFilter Distortion { get; }

    /// <summary>
    /// 
    /// </summary>
    ChannelMixFilter ChannelMix { get; }

    /// <summary>
    /// 
    /// </summary>
    LowPassFilter LowPass { get; }
}