using System;

namespace Victoria.Interfaces {
    /// <summary>
    /// 
    /// </summary>
    public interface ILavaTrack {
        /// <summary>
        /// 
        /// </summary>
        string Hash { get; }

        /// <summary>
        /// 
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 
        /// </summary>
        string Title { get; }

        /// <summary>
        /// 
        /// </summary>
        string Author { get; }
        
        /// <summary>
        /// 
        /// </summary>
        string Url { get; }
        
        /// <summary>
        /// 
        /// </summary>
        TimeSpan Position { get; }
        
        /// <summary>
        /// 
        /// </summary>
        TimeSpan Duration { get; }
        
        /// <summary>
        /// 
        /// </summary>
        bool IsSeekable { get; }
        
        /// <summary>
        /// 
        /// </summary>
        bool IsLiveStream { get; }
    }
}