using Lynx.Core.Abstractions;

namespace Lynx.Core;

public abstract class Module : IAsyncDisposable
{
    private uint _initialized = 0u;
    public abstract string Name { get; }
    public abstract bool AutoStart { get; }

    protected IModuleHost? Host { get; private set; }
    
    public void Initialize(IModuleHost host)
    {
        if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 0) {
            Host = host;
        }
        else {
            throw new InvalidOperationException($"Cannot activate module {Name} twice");
        }
    }

    public virtual Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task StopAsync() => Task.CompletedTask;

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
