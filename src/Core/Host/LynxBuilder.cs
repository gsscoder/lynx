using Lynx.Core.Modules;
using Microsoft.Extensions.DependencyInjection;
using SlimMessageBus;
using SlimMessageBus.Host;

namespace Lynx.Core.Host;

public sealed class LynxBuilder
{
    private IServiceCollection _services;
    private List<Type> _moduleTypes = new();

    internal LynxBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public LynxBuilder WithMessageBus(Action<MessageBusBuilder> configure)
    {
        _services.AddSlimMessageBus(configure);

        return this;
    }

    public LynxBuilder WithModule<T>() where T : IModule
    {
        _moduleTypes.Add(typeof(T));

        return this;
    }

    public LynxBuilder WithAudioCapture()
    {
        _services.AddSingleton<AudioCaptureModule>();

        return WithModule<AudioCaptureModule>();
    }

    //public LynxBuilder WithSpeechToTextModule()
    //{
    //    _services.AddSingleton<SpeechToTextModule>(sp =>
    //        new SpeechToTextModule(sp.GetRequiredService<IMessageBus>(), "ggml-base.bin"));

    //    return WithModule<AudioCaptureModule>();
    //}

    public void Build()
    {
        _services.AddSingleton<FrameworkHost>(sp =>
        {
            var host = new FrameworkHost(sp);
            foreach (var type in _moduleTypes) {
                host.RegisterModule(type);
            }
            return host;
        });
    }
}
