namespace Lynx.Core;

public interface IModule : IAsyncDisposable
{
    string Name { get; }

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync();
}
