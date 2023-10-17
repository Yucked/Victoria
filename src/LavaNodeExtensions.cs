using System.Threading.Tasks;

namespace Victoria;

/// <summary>
/// 
/// </summary>
public static class LavaNodeExtensions {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lavaNode"></param>
    /// <param name="guildId"></param>
    /// <typeparam name="TLavaPlayer"></typeparam>
    /// <typeparam name="TLavaTrack"></typeparam>
    /// <returns></returns>
    public static async Task<TLavaPlayer> TryGetPlayerAsync<TLavaPlayer, TLavaTrack>(
        this LavaNode<TLavaPlayer, TLavaTrack> lavaNode,
        ulong guildId)
        where TLavaTrack : LavaTrack
        where TLavaPlayer : LavaPlayer<TLavaTrack> {
        try {
            return await lavaNode.GetPlayerAsync(guildId);
        }
        catch {
            return null;
        }
    }
}