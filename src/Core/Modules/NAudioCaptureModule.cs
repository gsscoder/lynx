using Lynx.Core.Configuration;
using Lynx.Core.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using SlimMessageBus;

namespace Lynx.Core.Modules;

public sealed class NAudioCaptureModule : Module
{
    private readonly ILogger _logger;
    private readonly IMessageBus _bus;
    private readonly WaveInEvent _waveIn;
    private CancellationTokenSource? _cts;
    private int _started;

    public override string Name => "NAudioCapture";
    public override bool AutoStart => true; 

    public NAudioCaptureModule(ILogger<NAudioCaptureModule> logger,
        IOptions<WhishperSpeechSettings> options,
        IMessageBus bus)
    {
        _logger = logger;
        _bus = bus;
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 1), // 16kHz, mono
            BufferMilliseconds = options.Value.BufferMs,
        };
        _waveIn.DataAvailable += async (s, e) => await PublishAudioAsync(e);
        _started = 0;
    }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_started != 0) {
            throw new InvalidOperationException($"{Name} module already started");
        }

        _logger.LogInformation("Starting audio capture");
        
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _waveIn.StartRecording();
        Interlocked.Exchange(ref _started, 1);

        return Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        _logger.LogInformation("Stopping audio capture");
 
        _cts?.Cancel();
        _waveIn.StopRecording();
        Interlocked.Exchange(ref _started, 0);
        
        return Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        try {
            await StopAsync();
            _waveIn.Dispose();
            _cts?.Dispose();
        }
        catch { }
    }

    private async Task PublishAudioAsync(WaveInEventArgs e)
    {
        if (_cts?.IsCancellationRequested != false) return;
        
        var message = new AudioChunkMessage
        {
            AudioData = e.Buffer,
            BytesRecorded = e.BytesRecorded
        };
        await _bus.Publish(message);
    }
}
