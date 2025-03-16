using Microsoft.Extensions.Logging;

namespace Lynx.Core.Infastructure;

public sealed class FlushingLogger : ILogger
{
    private readonly ILogger _innerLogger;
    public FlushingLogger(ILogger innerLogger) => _innerLogger = innerLogger;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _innerLogger.Log(logLevel, eventId, state, exception, formatter);
        Console.Out.Flush();
    }

    public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _innerLogger.BeginScope(state);
}
