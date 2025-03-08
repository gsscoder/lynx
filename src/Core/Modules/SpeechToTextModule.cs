using Lynx.Core.Messages;
using Lynx.Core;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using SlimMessageBus;
using Whisper.net;
using Microsoft.Extensions.Options;
using Lynx.Core.Configuration;

public class SpeechToTextModule : IModule, IConsumer<AudioChunkMessage>
{
    private readonly ILogger<SpeechToTextModule> _logger;
    private readonly AudioSpeechSettings _settings;
    private readonly IMessageBus _bus;
    private readonly WhisperProcessor _processor;
    private bool _isListening = false;
    private readonly List<float> _audioBuffer = new();
    private const int MinSamples = 80000; // 5s * 16kHz

    public string Name => "SpeechToText";

    public SpeechToTextModule(ILogger<SpeechToTextModule> logger,
        IOptions<AudioSpeechSettings> options,
        IMessageBus bus)
    {
        _logger = logger;
        _settings = options.Value;
        _bus = bus;
        var factory = WhisperFactory.FromPath(_settings.ModelPath);
        _processor = factory.CreateBuilder().WithLanguage("en").Build();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync() => Task.CompletedTask;
    public ValueTask DisposeAsync() { _processor.Dispose(); return ValueTask.CompletedTask; }

    public async Task OnHandle(AudioChunkMessage message, CancellationToken cancellationToken)
    {
        var waveBuffer = new WaveBuffer(message.AudioData);
        var samples = new float[message.BytesRecorded / 2];
        for (int i = 0; i < samples.Length; i++) {
            float sample = waveBuffer.ShortBuffer[i] / 32768f; // [-1, 1]
            samples[i] = Math.Clamp(sample * 10f, -1f, 1f); // Amplify but clamp
        }

        _audioBuffer.AddRange(samples);
        //_logger.LogInformation("Buffered {Count} samples, Total: {Total}", samples.Length, _audioBuffer.Count);

        if (_audioBuffer.Count >= MinSamples) {
            float[] bufferedSamples = _audioBuffer.ToArray();
            _audioBuffer.Clear();

            //_logger.LogInformation("Processing {Count} samples, Max: {Max}", bufferedSamples.Length, bufferedSamples.Max());

            await foreach (var segment in _processor.ProcessAsync(bufferedSamples)) {
                string text = segment.Text.Trim().ToLowerInvariant();
                _logger.LogInformation("Heard: {Text}", text);

                if (!_isListening) {
                    if (text.Contains(_settings.ListenStartTrigger)) {
                        _isListening = true;
                        _logger.LogInformation("Listening started");
                    }
                }
                else {
                    await _bus.Publish(new TextMessage { Text = text });
                    if (string.IsNullOrWhiteSpace(text) || text.Contains(_settings.ListenEndTrigger)) {
                        _isListening = false;
                        _logger.LogInformation("Listening stopped");
                    }
                }
            }
        }
    }
}
