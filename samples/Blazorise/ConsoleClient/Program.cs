using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Client;
using Templates.Blazor2.Abstractions;
using Templates.Blazor2.Abstractions.Clients;
using static System.Console;

Write("Enter SessionId to use: ");
var sessionId = ReadLine()!.Trim();
var session = new Session(sessionId);

var services = CreateServiceProvider();
var todoService = services.GetRequiredService<ITodoService>();
var computed = await Computed.Capture(ct => todoService.GetSummary(session, ct));
while (true) {
    WriteLine($"- {computed.Value}");
    await computed.WhenInvalidated();
    computed = await computed.Update(false);
}

IServiceProvider CreateServiceProvider()
{
    // ReSharper disable once VariableHidesOuterVariable
    var services = new ServiceCollection();
    services.AddLogging(b => {
        b.ClearProviders();
        b.SetMinimumLevel(LogLevel.Warning);
        b.AddConsole();
    });

    var baseUri = new Uri("http://localhost:5005");
    var apiBaseUri = new Uri($"{baseUri}api/");

    var fusion = services.AddFusion();
    var fusionClient = fusion.AddRestEaseClient(
        (c, o) => {
            o.BaseUri = baseUri;
            o.MessageLogLevel = LogLevel.Information;
        }).ConfigureHttpClientFactory(
        (c, name, o) => {
            var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
            var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
            o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
        });
    fusionClient.AddReplicaService<ITodoService, ITodoClient>();
    fusion.AddAuthentication().AddRestEaseClient();

    // Default delay for update delayers
    services.AddSingleton(c => new UpdateDelayer.Options() {
        DelayDuration = TimeSpan.FromSeconds(0.1),
    });
    return services.BuildServiceProvider();
}
