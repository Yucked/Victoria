using System;
using Victoria.Rest;

namespace Victoria.Interfaces {
    /// <inheritdoc />
    public interface ILavaPlayer : ILavaPlayer<ILavaTrack> { }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TLavaTrack"></typeparam>
    public interface ILavaPlayer<out TLavaTrack> : IAsyncDisposable
        where TLavaTrack : ILavaTrack {
        /// <summary>
        /// 
        /// </summary>
        public ulong GuildId { get; }

        /// <summary>
        /// 
        /// </summary>
        TLavaTrack Track { get; }
        
        /// <summary>
        /// 
        /// </summary>
        int Volume { get; }
        
        /// <summary>
        /// 
        /// </summary>
        bool IsPaused { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IFilters Filters { get; }
        
        /// <summary>
        /// 
        /// </summary>
        VoiceState State { get; }
    }
}