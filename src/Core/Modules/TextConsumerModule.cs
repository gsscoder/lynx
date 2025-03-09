using Lynx.Core.Messages;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Lynx.Core.Modules;

public class TextConsumerModule(ILogger<TextConsumerModule> logger)
    : Module, IConsumer<TextMessage>
{
    private readonly ILogger _logger = logger;

    public override string Name => "TextConsumer";
    public override bool IsStartable => true;

    public override Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public override Task StopAsync() => Task.CompletedTask;
    public override ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task OnHandle(TextMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Collected: {message.Text}");

        return Task.CompletedTask;
    }
}
