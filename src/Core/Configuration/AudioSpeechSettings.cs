using System.ComponentModel.DataAnnotations;

namespace Lynx.Core.Configuration;

public sealed class AudioSpeechSettings
{
    public const string SectionKey = "AudioSpeech";

    [Required(ErrorMessage = $"{nameof(BufferMs)} setting is mandatory")]
    [Range(10, 3000, ErrorMessage = $"{nameof(BufferMs)} setting must range from 10 to 3000")]
    public required int BufferMs { get; init; } = Defaults.AudioBufferMs;

    [Required(ErrorMessage = $"{nameof(SilenceThreshold)} setting is mandatory")]
    [Range(1, 100000, ErrorMessage = $"{nameof(BufferMs)} setting must range from 10 to 100000")]
    public required int MinSamples { get; init; } = Defaults.MinSamples; // Xs * 16kHz

    [Required(ErrorMessage = $"{nameof(SilenceThreshold)} setting is mandatory")]
    [Range(1, 50, ErrorMessage = $"{nameof(BufferMs)} setting must range from 0 to 50")]
    public required float SilenceThreshold { get; init; } = Defaults.SilenceThreshold;

    [Required(ErrorMessage = $"{nameof(ModelPath)} setting is mandatory")]
    public required string ModelPath { get; init; }
        
    [Required(ErrorMessage = $"{nameof(ListenStartTrigger)} setting is mandatory")]
    public required string ListenStartTrigger { get; init; }
}
