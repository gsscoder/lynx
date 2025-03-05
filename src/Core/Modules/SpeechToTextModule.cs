using Core.Messages;
using Core;
using NAudio.Wave;
using SlimMessageBus;
using Whisper.net;
using Microsoft.Extensions.Logging;

namespace Core.Modules;

public class SpeechToTextModule : IModule, IConsumer<AudioChunkMessage>
{
    private readonly ILogger _logger;
    private readonly IMessageBus _bus;
    private readonly WhisperProcessor _processor;
    private bool _isRecording = false;
    private const string _triggerPhrase = "hey Lynx";

    public string Name => "SpeechToText";

    public SpeechToTextModule(ILogger<SpeechToTextModule> logger,
        IMessageBus bus,
        string modelPath)
    {
        _logger = logger;
        _bus = bus;
        var factory = WhisperFactory.FromPath(modelPath);
        _processor = factory.CreateBuilder().WithLanguage("en").Build();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
   
    public Task StopAsync() => Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        _processor.Dispose();
    }

    public async Task OnHandle(AudioChunkMessage message)
    {
        var waveBuffer = new WaveBuffer(message.AudioData);
        var samples = new float[message.BytesRecorded / 2];
        for (int i = 0; i < samples.Length; i++)
            samples[i] = waveBuffer.ShortBuffer[i] / 32768f;

        await foreach (var segment in _processor.ProcessAsync(samples)) {
            string text = segment.Text.Trim().ToLowerInvariant();
            _logger.LogInformation($"Heard: {text}");

            if (!_isRecording) {
                if (text.Contains(_triggerPhrase)) {
                    _isRecording = true;
                    _logger.LogInformation("Trigger detected. Recording");
                }
            }
            else {
                await _bus.Publish(new TextMessage { Text = text });
                if (string.IsNullOrWhiteSpace(text) || text.Contains("stop boy")) {
                    _isRecording = false;
                    _logger.LogInformation("Stopped recording");
                }
            }
        }
    }

    public Task OnHandle(AudioChunkMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
