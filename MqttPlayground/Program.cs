using MqttPlayground;
using System.Diagnostics;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        // Configure logging
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<Stopwatch>();
        services.AddHostedService<MqttPublisher>();
        services.AddHostedService<MqttSubscriber>();
        services.AddHostedService<MqttManagedClient>();
    })
    .Build();

host.Run();
