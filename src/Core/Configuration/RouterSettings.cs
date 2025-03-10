using System.ComponentModel.DataAnnotations;

namespace Lynx.Core.Configuration;

public sealed class ModuleBinding
{
    [Required(ErrorMessage = $"{nameof(TypeName)} setting is mandatory")]
    public required string TypeName { get; init; }
}

public sealed class RouterSettings
{
}
