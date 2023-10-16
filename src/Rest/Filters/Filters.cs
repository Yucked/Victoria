using System.Collections.Generic;

namespace Victoria.Rest.Filters;

/// <summary>
/// 
/// </summary>
public struct Filters {
    /// <summary>
    /// 
    /// </summary>
    public float Volume { get;  init; }

    /// <summary>
    /// 
    /// </summary>
    public IEnumerable<EqualizerBand> Bands { get;  init; }

    /// <summary>
    /// 
    /// </summary>
    public KarokeFilter Karoke { get;  init; }

    /// <summary>
    /// 
    /// </summary>
    public TimescaleFilter Timescale { get;  init; }

    /// <summary>
    /// 
    /// </summary>
    public TremoloFilter Tremolo { get;  init; }

    /// <summary>
    /// 
    /// </summary>
    public VibratoFilter Vibrato { get;  init; }

    /// <summary>
    /// 
    /// </summary>
    public RotationFilter Rotation { get;  init; }

    /// <summary>
    /// 
    /// </summary>
    public DistortionFilter Distortion { get;  init; }

    /// <summary>
    /// 
    /// </summary>
    public ChannelMixFilter ChannelMix { get;  init; }

    /// <summary>
    /// 
    /// </summary>
    public LowPassFilter LowPass { get;  init; }
}