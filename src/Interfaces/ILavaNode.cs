using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria.Responses.Search;
using Victoria.Wrappers;

namespace Victoria.Interfaces {
    /// <inheritdoc />
    public interface ILavaNode : ILavaNode<ILavaPlayer> { }

    /// <inheritdoc />
    public interface ILavaNode<TLavaPlayer> : ILavaNode<TLavaPlayer, ILavaTrack>
        where TLavaPlayer : ILavaPlayer<ILavaTrack> { }

    /// <summary>
    /// 
    /// </summary>
    public interface ILavaNode<TLavaPlayer, TLavaTrack> : IAsyncDisposable
        where TLavaPlayer : ILavaPlayer<TLavaTrack>
        where TLavaTrack : ILavaTrack {
        /// <summary>
        /// 
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 
        /// </summary>
        IReadOnlyCollection<TLavaPlayer> Players { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ValueTask ConnectAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ValueTask DisconnectAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ValueTask<SearchResponse> SearchAsync(SearchType searchType, string query);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        bool HasPlayer(ulong guildId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="lavaPlayer"></param>
        /// <returns></returns>
        bool TryGetPlayer(ulong guildId, out TLavaPlayer lavaPlayer);
    }
}