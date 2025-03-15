namespace Lynx.Core.Messages;

public class AudioChunkMessage
{
    public required byte[] AudioData { get; init; }

    public required int BytesRecorded { get; init; }
}

