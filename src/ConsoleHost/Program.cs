using Lynx.Core;
using Lynx.Core.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlimMessageBus.Host.Memory;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .ConfigureServices(services =>
    {
        services.AddLynx(lb =>
        {
            lb.WithMessageBus(mbb =>
            {
                mbb.WithProviderMemory();
                mbb.Produce<AudioChunkMessage>(x => x.DefaultTopic("audio-chunks"));
                mbb.Produce<TextMessage>(x => x.DefaultTopic("text-messages"));
                //mbb.Consume<AudioChunkMessage>(x => x.WithConsumer<SpeechToTextModule>());
                //mbb.Consume<TextMessage>(x => x.WithConsumer<TextConsumerModule>());
            });
            lb.WithAudioCapture();
        });
    })
    .Build();

var framework = host.Services.GetRequiredService<FrameworkHost>();
await framework.StartAsync();

Console.WriteLine("Press Enter to stop...");
Console.ReadLine();

await framework.DisposeAsync();
