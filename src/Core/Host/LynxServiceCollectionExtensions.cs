using Lynx.Core;
using Lynx.Core.Host;
using SlimMessageBus.Host;

namespace Microsoft.Extensions.DependencyInjection;

public static class LynxServiceCollectionExtensions
{
    public static IServiceCollection AddLynx(this IServiceCollection services,
        Action<LynxBuilder> configure)
    {
        var builder = new LynxBuilder(services);
        configure(builder);
        builder.Build();

        return services;
    }
}
