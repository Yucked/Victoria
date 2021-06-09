using System;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Victoria.DNET;

namespace Victoria.Tests {
    public static class Global {
        public static readonly IServiceProvider Provider
            = new ServiceCollection()
                .AddLogging()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<LavaNode>()
                //.AddLavaNode<LavaNode>()
                //.AddLavaNode()
                .BuildServiceProvider();
    }
}