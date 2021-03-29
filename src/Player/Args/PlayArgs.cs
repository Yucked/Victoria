using System;

namespace Victoria.Player.Args {
    /// <summary>
    /// 
    /// </summary>
    public struct PlayArgs<T> {
        /// <summary>
        /// 
        /// </summary>
        public T Track { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool NoReplace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ShouldPause { get; set; }

        /// <summary>
        ///
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan EndTime { get; set; }
    }
}
