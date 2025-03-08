using Lynx.Core.Messages;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Lynx.Core.Modules;

public class TextConsumerModule(ILogger<TextConsumerModule> logger) : IModule, IConsumer<TextMessage>
{
    private readonly ILogger _logger = logger;

    public string Name => "TextConsumer";
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync() => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task OnHandle(TextMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Collected: {message.Text}");

        return Task.CompletedTask;
    }
}
