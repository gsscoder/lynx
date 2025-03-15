using Lynx.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using SharpX.Extensions;

namespace Lynx.Core;

public sealed class FrameworkHost : IModuleHost, IAsyncDisposable
{
    private readonly IServiceProvider _services;
    private readonly List<Module> _modules = new();
    private CancellationTokenSource? _cts;

    public FrameworkHost(IServiceProvider services)
    {
        _services = services;

    }

    public void RegisterModule<T>() where T : Module
    {
        var module = _services.GetRequiredService<T>();
        module.Initialize(this);
        _modules.Add(module);
    }

    public Module GetModule(string name)
    {
        var module = _modules.SingleOrDefault(m => m.Name.EqualsIgnoreCase(name));
        if (module == null) {
            throw new InvalidOperationException($"Module {name} is not found.");
        }

        return module;
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        foreach (var module in _modules.Where(m => m.AutoStart))
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
