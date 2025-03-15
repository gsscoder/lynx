using SharpX.Extensions;

namespace Lynx.Core.Utilities;

public static class AudioUtility
{
    public static bool IsBlank(string text) => text.IsEmpty() || text.EqualsIgnoreCase("[blank_audio]");
}
