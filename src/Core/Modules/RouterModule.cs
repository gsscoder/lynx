using Lynx.Core.Configuration;
using Lynx.Core.Infastructure;
using Lynx.Core.Messages;
using Lynx.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Lynx.Core.Modules;

public class RouterModule : Module, IConsumer<TextMessage>
{
    private readonly ILogger _logger;
    private readonly ISimilarStringFinder _simStrFinder;
    private readonly IEnumerable<(string, string)> _bindings;

    public RouterModule(ILogger<RouterModule> logger,
        IOptions<RouterSettings> settings,
        ISimilarStringFinder simStrFinder)
    {
        _logger = logger;
        _simStrFinder = simStrFinder;
        _bindings = settings.Value.Bindings.SelectMany(b => b.Commands.Select(t => (t, b.Module)));
    }

    public override string Name => "Router";
    public override bool AutoStart => true;

    public override Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public override Task StopAsync() => Task.CompletedTask;
    public override ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public async Task OnHandle(TextMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Text: {message.Text}");

        if (AudioUtility.IsBlank(message.Text)) return;

        foreach (var binding in _bindings) {
            if (_simStrFinder.FindSimilar(binding.Item1, message.Text).Any()) {
                var module = Host!.GetModule(binding.Item2);
                await module.StartAsync(cancellationToken);
                break;
            }
        }
    }
}
