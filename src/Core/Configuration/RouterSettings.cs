using System.ComponentModel.DataAnnotations;

namespace Lynx.Core.Configuration;

public sealed class RouterSettings
{
    public const string SectionKey = "Router";

    public sealed class Binding
    {
        [Required(ErrorMessage = $"{nameof(Commands)} setting is mandatory")]
        public required IEnumerable<string> Commands { get; init; }

        [Required(ErrorMessage = $"{nameof(Module)} setting is mandatory")]
        public required string Module { get; init; }
    }


    [Required(ErrorMessage = $"{nameof(Bindings)} setting is mandatory")]
    public required IEnumerable<Binding> Bindings { get; init; }

    public string? FallbackModule { get; init; }
}
