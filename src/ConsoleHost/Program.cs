using Lynx.Core;
using Lynx.Core.Infastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        logging.AddProvider(new FlushingLoggerProvider(
        new ConsoleLoggerProvider(
            new OptionsMonitor<ConsoleLoggerOptions>(
                new OptionsFactory<ConsoleLoggerOptions>(
                    [new ConfigureOptions<ConsoleLoggerOptions>(options => {})],
                    []),
                [], new OptionsCache<ConsoleLoggerOptions>())
            )
        ));
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddLynx(builder =>
        {
            builder.AddWhisperSpeechToText()
                   .AddRouter()
                   .AddSystemInfo();
        });
    })
    .Build();

var framework = host.Services.GetRequiredService<FrameworkHost>();
await framework.StartAsync();

Console.ReadLine();

await framework.DisposeAsync();
