using System.Text.Json.Serialization;

namespace Victoria.Player.Filters {
    /// <summary>
    /// Rotates the sound around the stereo channels/user headphones aka Audio Panning.
    /// It can produce an effect similar to: https://youtu.be/QB9EB8mTKcc (without the reverb)
    /// </summary>
    public struct RotationFilter : IFilter {
        /// <summary>
        /// The frequency of the audio rotating around the listener in Hz. 0.2 is similar to the example video above.
        /// </summary>
        [JsonPropertyName("rotationHz")]
        public double Hertz { get; set; }
    }
}