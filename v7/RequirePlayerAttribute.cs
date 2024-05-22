using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Victoria;

namespace GenIVIV.Modules;

public class RequirePlayerAttribute : PreconditionAttribute {
    public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
                                                                         CommandInfo command,
                                                                         IServiceProvider provider) {
        var lavaNode = provider.GetRequiredService<LavaNode<LavaPlayer<LavaTrack>, LavaTrack>>();
        var player = await lavaNode.TryGetPlayerAsync(context.Guild.Id);
        return player == null
            ? await Task.FromResult(PreconditionResult.FromError("I'm not connected to a voice channel."))
            : await Task.FromResult(PreconditionResult.FromSuccess());
    }
}
