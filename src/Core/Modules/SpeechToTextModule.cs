using Lynx.Core;
using Lynx.Core.Configuration;
using Lynx.Core.Messages;
using Lynx.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using SharpX.Extensions;
using SlimMessageBus;
using Whisper.net;

namespace Lynx.Core.Modules;

public sealed class SpeechToTextModule : Module, IConsumer<AudioChunkMessage>
{
    private readonly ILogger<SpeechToTextModule> _logger;
    private readonly AudioSpeechSettings _settings;
    private readonly ISimilarStringFinder _simStrFinder;
    private readonly IMessageBus _bus;
    private readonly WhisperProcessor _processor;
    private bool _isListening = false;
    private readonly List<float> _audioBuffer = new();
    private const int MinSamples = 80000; // 5s * 16kHz

    public override string Name => "SpeechToText";
    public override bool AutoStart => true;

    public SpeechToTextModule(ILogger<SpeechToTextModule> logger,
        IOptions<AudioSpeechSettings> options,
        IMessageBus bus,
        ISimilarStringFinder simStringFinder)
    {
        _logger = logger;
        _settings = options.Value;
        _bus = bus;
        _simStrFinder = simStringFinder;
        var factory = WhisperFactory.FromPath(_settings.ModelPath);
        _processor = factory.CreateBuilder().WithLanguage("en").Build();
    }

    public override Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public override Task StopAsync() => Task.CompletedTask;
    public override ValueTask DisposeAsync() { _processor.Dispose(); return ValueTask.CompletedTask; }

    public async Task OnHandle(AudioChunkMessage message, CancellationToken cancellationToken)
    {
        var waveBuffer = new WaveBuffer(message.AudioData);
        var samples = new float[message.BytesRecorded / 2];
        for (int i = 0; i < samples.Length; i++) {
            var sample = waveBuffer.ShortBuffer[i] / 32768f; // [-1, 1]
            samples[i] = Math.Clamp(sample * 10f, -1f, 1f); // Amplify but clamp
        }

        _audioBuffer.AddRange(samples);

        if (_audioBuffer.Count >= MinSamples) {
            var bufferedSamples = _audioBuffer.ToArray();
            _audioBuffer.Clear();

            await foreach (var segment in _processor.ProcessAsync(bufferedSamples)) {
                var text = segment.Text.Trim().ToLowerInvariant();
                var firstText = String.Empty;
                _logger.LogInformation("Heard: {Text}", text);

                if (!_isListening) {
                    if (text.Contains(_settings.ListenStartTrigger)) {
                        _isListening = true;
                        firstText = _simStrFinder.ReplaceSimilar(text,
                            _settings.ListenStartTrigger, String.Empty).Trim();
                        _logger.LogInformation(">> Listening started");
                    }
                }
                else {
                    var seperator = firstText.IsEmpty() ? String.Empty : " ";
                    await _bus.Publish(new TextMessage { Text = $"{firstText}{seperator}{text.Trim()}" });
                    if (text.IsEmpty() || text == "[blank_audio]") {
                        _isListening = false;
                        _logger.LogInformation(">> Listening stopped");
                    }
                }
            }
        }
    }
}
