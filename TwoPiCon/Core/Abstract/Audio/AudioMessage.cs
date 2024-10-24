using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoPiCon.Core.Abstract.Audio;

public class AudioMessage
{
    public AudioMessage(byte[] audioData, int byteCount)
    {
        AudioData = audioData;
        ByteCount = byteCount;
    }

    public byte[] AudioData { get; set; }
    public int ByteCount { get; set; }
}
