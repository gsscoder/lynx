namespace Lynx.Core.Messages;

public class AudioChunkMessage
{
    public byte[] AudioData { get; set; }

    public int BytesRecorded { get; set; }
}

