namespace Victoria.Frostbyte.Infos
{
    /// <summary>
    ///     Track author.
    /// </summary>
    public struct AuthorInfo
    {
        /// <summary>
        ///     Author name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Author channel/page url.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        ///     If author has an avatar url.
        /// </summary>
        public string AvatarUrl { get; private set; }
    }
}