namespace Victoria.Configs
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class EndpointConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Authorization { get; set; }

        /// <inheritdoc cref="EndpointConfig" />
        public EndpointConfig()
        {
            Host ??= "127.0.0.1";
            Port = Port is 0 ? 2333 : Port;
            Authorization ??= "youshallnotpass";
        }
    }
}