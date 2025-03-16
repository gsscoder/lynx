using Lynx.Core.Configuration;
using Lynx.Core.Modules;
using Lynx.Core;
using Lynx.Core.Messages;
using SlimMessageBus.Host;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.DependencyInjection;

public interface ILynxBuilder
{
    ILynxBuilder AddAudioCapture();
    ILynxBuilder AddSpeechToText();
    ILynxBuilder AddRouter();
    ILynxBuilder AddSystemInfo();
}

internal class LynxBuilder : ILynxBuilder
{
    private readonly IServiceCollection _services;
    public List<Action<MessageBusBuilder>> SmbConfigs { get; } = [];
    public List<Type> ModuleTypes { get; } = [];

    public LynxBuilder(IServiceCollection services) => _services = services;

    public ILynxBuilder AddAudioCapture()
    {
        SmbConfigs.Add(mbb => mbb.Produce<AudioChunkMessage>(x => x.DefaultTopic("audio-chunks")));
        _services.AddSingleton<AudioCaptureModule>();
        RegisterModule<AudioCaptureModule>();
        return this;
    }

    public ILynxBuilder AddSpeechToText()
    {
        SmbConfigs.Add(mbb =>
        {
            mbb.Consume<AudioChunkMessage>(x => x.Topic("audio-chunks").WithConsumer<SpeechToTextModule>());
            mbb.Produce<TextMessage>(x => x.DefaultTopic("text-messages"));
        });
        _services.AddOptions<AudioSpeechSettings>().BindConfiguration(AudioSpeechSettings.SectionKey);
        _services.AddSingleton<SpeechToTextModule>();
        RegisterModule<SpeechToTextModule>();
        return this;
    }

    public ILynxBuilder AddRouter()
    {
        SmbConfigs.Add(mbb => mbb.Consume<TextMessage>(x => x.Topic("text-messages").WithConsumer<RouterModule>()));
        _services.AddOptions<RouterSettings>().BindConfiguration(RouterSettings.SectionKey);
        _services.AddSingleton<RouterModule>();
        RegisterModule<RouterModule>();
        return this;
    }

    public ILynxBuilder AddSystemInfo()
    {
        _services.AddSingleton<SystemInfoModule>();
        RegisterModule<SystemInfoModule>();
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RegisterModule<T>() where T : Module => ModuleTypes.Add(typeof(T));
}
