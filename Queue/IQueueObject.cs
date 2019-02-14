namespace Victoria.Queue
{
    /// <summary>
    /// Every object that needs to be in BaseQueue should inherit from <see cref="IQueueObject" />.
    /// </summary>
    public interface IQueueObject
    {
        /// <summary>
        /// Unique identifier for this object.
        /// </summary>
        string Id { get; }
    }
}