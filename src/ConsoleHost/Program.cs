using Lynx.Core;
using Lynx.Core.Messages;
using Lynx.Core.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlimMessageBus.Host;
using SlimMessageBus;
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
        services.AddSlimMessageBus(mbb =>
        {
            mbb.WithProviderMemory();
            mbb.Produce<AudioChunkMessage>(x => x.DefaultTopic("audio-chunks"));
            mbb.Produce<TextMessage>(x => x.DefaultTopic("text-messages"));
            mbb.Consume<AudioChunkMessage>(x => x.WithConsumer<SpeechToTextModule>());
            //mbb.Consume<TextMessage>(x => x.WithConsumer<TextConsumerModule>());
        });

        services.AddSingleton<AudioCaptureModule>();
        services.AddSingleton<SpeechToTextModule>(sp =>
            new SpeechToTextModule(sp.GetRequiredService<ILogger<SpeechToTextModule>>(),
                sp.GetRequiredService<IMessageBus>(), "ggml-base.bin"));
        //services.AddSingleton<TextConsumerModule>();

        services.AddSingleton<FrameworkHost>(sp =>
        {
            var host = new FrameworkHost(sp);
            host.RegisterModule<AudioCaptureModule>();
            host.RegisterModule<SpeechToTextModule>();
            //host.RegisterModule<TextConsumerModule>();
            return host;
        });
    })
    .Build();

var framework = host.Services.GetRequiredService<FrameworkHost>();
await framework.StartAsync();

Console.WriteLine("Press Enter to stop...");
Console.ReadLine();

await framework.DisposeAsync();
