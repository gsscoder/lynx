using SharpX.Extensions;

namespace Lynx.Core.Infastructure;

public static class AudioUtility
{
    public static bool IsBlank(string text) => text.IsEmpty() || text.EqualsIgnoreCase("[blank_audio]");
}
