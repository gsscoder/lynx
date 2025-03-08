using System.ComponentModel.DataAnnotations;

namespace Lynx.Core.Configuration;

public sealed class AudioSpeechSettings
{
    public const string SectionKey = "AudioSpeech";

    [Required(ErrorMessage = $"{nameof(BufferMs)} setting is mandatory")]
    [Range(10, 3000, ErrorMessage = $"{nameof(BufferMs)} setting must range from 10 to 3000")]
    public required int BufferMs { get; set; } = Defaults.AudioBufferMs;

    [Required(ErrorMessage = $"{nameof(ModelPath)} setting is mandatory")]
    public required string ModelPath { get; set; }
        
    [Required(ErrorMessage = $"{nameof(ListenStartTrigger)} setting is mandatory")]
    public required string ListenStartTrigger { get; set; }
}
