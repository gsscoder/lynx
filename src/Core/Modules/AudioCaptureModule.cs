using Lynx.Core.Messages;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using SlimMessageBus;

namespace Lynx.Core.Modules;

public class AudioCaptureModule : IModule
{
    private readonly ILogger _logger;
    private readonly IMessageBus _bus;
    private readonly WaveInEvent _waveIn;
    private CancellationTokenSource? _cts;
    private int _started;

    public string Name => "AudioCapture";

    public AudioCaptureModule(ILogger<AudioCaptureModule> logger, IMessageBus bus)
    {
        _logger = logger;
        _bus = bus;
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 1), // 16kHz, mono
            BufferMilliseconds = 300
        };
        _waveIn.DataAvailable += async (s, e) => await PublishAudioAsync(e);
        _started = 0;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
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

    public Task StopAsync()
    {
        _logger.LogInformation("Stopping audio capture");
 
        _cts?.Cancel();
        _waveIn.StopRecording();
        Interlocked.Exchange(ref _started, 0);
        
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
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