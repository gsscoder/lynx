using Lynx.Core.Abstractions;
using Lynx.Core.Infastructure;
using Microsoft.Extensions.Configuration;

namespace Lynx.Core.Services;

public sealed class AudioUtilityFactory(IConfiguration config)
{
    public IAudioUtility CreateAudioUtility()
    {
        return config.GetSection("speechToText").Get<string>().ToLowerInvariant() switch
        {
            "whishper" => new WhisperAudioUtility(),
            _ => throw new InvalidOperationException()
        };
    }
}
