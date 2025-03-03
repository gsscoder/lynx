using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Messages;

public class AudioChunkMessage
{
    public byte[] AudioData { get; set; }

    public int BytesRecorded { get; set; }
}

