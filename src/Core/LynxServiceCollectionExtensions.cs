using Core.Messages;
using Core.Modules;
using Lynx.Core;
using SlimMessageBus;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

namespace Microsoft.Extensions.DependencyInjection;

public static class LynxServiceCollectionExtensions
{
    public static IServiceCollection AddLynx(this IServiceCollection services)
    {
        services.AddSlimMessageBus(mbb =>
        {
            mbb.WithProviderMemory();
            mbb.Produce<AudioChunkMessage>(x => x.DefaultTopic("audio-chunks"));
            //mbb.Produce<TextMessage>(x => x.DefaultTopic("text-messages"));
            //mbb.Consume<AudioChunkMessage>(x => x.WithConsumer<SpeechToTextModule>());
            //mbb.Consume<TextMessage>(x => x.WithConsumer<TextConsumerModule>());
        });

        services.AddSingleton<AudioCaptureModule>();
        //services.AddSingleton<SpeechToTextModule>(sp =>
        //    new SpeechToTextModule(sp.GetRequiredService<IMessageBus>(), "ggml-base.bin"));
        //services.AddSingleton<TextConsumerModule>();

        services.AddSingleton<FrameworkHost>(sp =>
        {
            var host = new FrameworkHost(sp);
            host.RegisterModule<AudioCaptureModule>();
            //host.RegisterModule<SpeechToTextModule>();
            //host.RegisterModule<TextConsumerModule>();
            return host;
        });

        return services;
    }
}
