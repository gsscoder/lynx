using Lynx.Core;
using Lynx.Core.Configuration;
using Lynx.Core.Messages;
using Lynx.Core.Modules;
using SlimMessageBus.Host;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.DependencyInjection;

public interface ILynxBuilder
{
    ILynxBuilder AddWhisperSpeechToText();
    ILynxBuilder AddRouter();
    ILynxBuilder AddSystemInfo();
}

internal class LynxBuilder : ILynxBuilder
{
    private readonly IServiceCollection _services;
    public List<Action<MessageBusBuilder>> SmbConfigs { get; } = [];
    public List<Type> ModuleTypes { get; } = [];

    public LynxBuilder(IServiceCollection services) => _services = services;

    public ILynxBuilder AddWhisperSpeechToText()
    {
        SmbConfigs.Add(mbb =>
        {
            mbb.Produce<AudioChunkMessage>(x => x.DefaultTopic("audio-chunks"));
            mbb.Consume<AudioChunkMessage>(x => x.Topic("audio-chunks").WithConsumer<WhisperSTTModule>());
            mbb.Produce<TextMessage>(x => x.DefaultTopic("text-messages"));
        });
        _services.AddOptions<WhishperSpeechSettings>().BindConfiguration(WhishperSpeechSettings.SectionKey);
        _services.AddSingleton<NAudioCaptureModule>();
        _services.AddSingleton<WhisperSTTModule>();
        RegisterModule<WhisperSTTModule>();
        RegisterModule<NAudioCaptureModule>();

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
