using System;
using System.Net.Http;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Victoria.Tests;

public static class Globals {
    public static readonly IServiceProvider Provider
        = new ServiceCollection()
            .AddSingleton<HttpClient>()
            .AddSingleton<LavaNode<LavaPlayer, LavaTrack>>()
            .AddSingleton<Configuration>()
            .AddSingleton<BaseSocketClient, DiscordSocketClient>()
            .AddLogging(x => x.SetMinimumLevel(LogLevel.Trace))
            .BuildServiceProvider();

    public static LavaNode<LavaPlayer, LavaTrack> Node
        = Provider.GetRequiredService<LavaNode<LavaPlayer, LavaTrack>>();
}