using Lynx.Core.Services;
using Lynx.Core;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

namespace Microsoft.Extensions.DependencyInjection;

public static class LynxServiceExtensions
{
    public static IServiceCollection AddLynx(this IServiceCollection services, Action<ILynxBuilder> configure)
    {
        var builder = new LynxBuilder(services);
        configure(builder);

        services.AddSlimMessageBus(mbb =>
        {
            mbb.WithProviderMemory();
            foreach (var config in builder.SmbConfigs)
                config(mbb);
        });

        services.AddSingleton<FrameworkHost>(sp =>
            new FrameworkHost(sp, builder.ModuleTypes));
        services.AddSingleton<ISimilarStringFinder, SimilarStringFinder>();

        return services;
    }
}
