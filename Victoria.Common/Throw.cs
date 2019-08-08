using System;

namespace Victoria.Common
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct Throw
    {
        /// <summary>
        /// 
        /// </summary>
        public static void NotImplemented()
            => throw new NotImplementedException();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void InvalidOperation(string message)
            => throw new InvalidOperationException(message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="message"></param>
        public static void OutOfRange(string paramName, string message)
            => throw new ArgumentOutOfRangeException(paramName, message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="message"></param>
        public static void ArgNull(string paramName, string message)
            => throw new ArgumentNullException(paramName, message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void Exception(string message)
            => throw new Exception(message);
    }
}
