using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Victoria.Rest;

namespace Victoria.Tests;

public static class Globals {
    public static IServiceProvider Provider
        = new ServiceCollection()
            .AddSingleton<HttpClient>()
            .AddSingleton<LavaRest>()
            .AddSingleton<Configuration>()
            .AddLogging(x => x.SetMinimumLevel(LogLevel.Trace))
            .BuildServiceProvider();

    public static LavaRest Rest
        = Provider.GetRequiredService<LavaRest>();
}