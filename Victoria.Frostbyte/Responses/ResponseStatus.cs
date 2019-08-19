using Newtonsoft.Json;

namespace Victoria.Frostbyte.Responses
{
    /// <summary>
    /// </summary>
    public struct ResponseStatus
    {
        /// <summary>
        /// </summary>
        [JsonProperty("isSuccess")]
        public bool IsSuccess { get; internal set; }

        /// <summary>
        /// </summary>
        [JsonProperty("reason")]
        public string Reason { get; internal set; }
    }
}
