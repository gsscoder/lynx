using Microsoft.Extensions.DependencyInjection;

namespace Lynx.Core;

public sealed class FrameworkHost : IAsyncDisposable
{
    private readonly IServiceProvider _services;
    private readonly List<IModule> _modules = new();
    private CancellationTokenSource? _cts;

    public FrameworkHost(IServiceProvider services)
    {
        _services = services;
    }

    public void RegisterModule<T>() where T : IModule => _modules.Add(_services.GetRequiredService<T>());

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        foreach (var module in _modules)
            await module.StartAsync(_cts.Token);
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();
        foreach (var module in _modules)
            await module.StopAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        foreach (var module in _modules)
            await module.DisposeAsync();
        _cts?.Dispose();
    }
}
