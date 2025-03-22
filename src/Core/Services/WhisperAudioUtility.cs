using Lynx.Core.Abstractions;
using SharpX.Extensions;

namespace Lynx.Core.Infastructure;

public sealed class WhisperAudioUtility : IAudioUtility
{
    public bool IsBlank(string text) => text.IsEmpty() || text.EqualsIgnoreCase("[blank_audio]");
}
