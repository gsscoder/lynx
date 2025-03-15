using Microsoft.Extensions.Logging;

namespace Lynx.Core.Modules;

public sealed class SystemInfoModule(ILogger<SystemInfoModule> logger) : Module
{
    public override string Name => "SystemInfo";

    public override bool AutoStart => false;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Lynx, version {Version}", GetType().Assembly.GetName().Version);

        return Task.CompletedTask;
    }
}
