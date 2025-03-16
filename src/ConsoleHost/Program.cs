using Lynx.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddLynx(builder =>
        {
            builder.AddAudioCapture()
                   .AddSpeechToText()
                   .AddRouter()
                   .AddSystemInfo();
        });
    })
    .Build();

var framework = host.Services.GetRequiredService<FrameworkHost>();
await framework.StartAsync();

Console.ReadLine();

await framework.DisposeAsync();
