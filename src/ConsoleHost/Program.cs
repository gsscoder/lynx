using Lynx.Core;
using Lynx.Core.Configuration;
using Lynx.Core.Messages;
using Lynx.Core.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSlimMessageBus(mbb =>
        {
            mbb.WithProviderMemory();
            mbb.Produce<AudioChunkMessage>(x => x.DefaultTopic("audio-chunks"));
            mbb.Produce<TextMessage>(x => x.DefaultTopic("text-messages"));
            mbb.Consume<AudioChunkMessage>(x => x
                .Topic("audio-chunks")
                .WithConsumer<SpeechToTextModule>());
            mbb.Consume<TextMessage>(x => x
                .Topic("text-messages")
                .WithConsumer<TextConsumerModule>());
        });

        services.AddSingleton<FrameworkHost>();

        services.AddOptions<AudioSpeechSettings>()
            .Bind(context.Configuration.GetSection(AudioSpeechSettings.SectionKey));

        services.AddSingleton<AudioCaptureModule>();
        services.AddSingleton<SpeechToTextModule>();
        services.AddSingleton<TextConsumerModule>();
    })
    .Build();

var framework = host.Services.GetRequiredService<FrameworkHost>();
framework.RegisterModule<AudioCaptureModule>();
framework.RegisterModule<SpeechToTextModule>();
framework.RegisterModule<TextConsumerModule>();
await framework.StartAsync();

Console.ReadLine();

await framework.DisposeAsync();
