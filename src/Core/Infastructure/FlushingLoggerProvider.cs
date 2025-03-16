using Microsoft.Extensions.Logging;

namespace Lynx.Core.Infastructure;

public class FlushingLoggerProvider : ILoggerProvider
{
    private readonly ILoggerProvider _innerProvider;

    public FlushingLoggerProvider(ILoggerProvider innerProvider) => _innerProvider = innerProvider;

    public ILogger CreateLogger(string categoryName) => new FlushingLogger(_innerProvider.CreateLogger(categoryName));

    public void Dispose() => _innerProvider.Dispose();
}
