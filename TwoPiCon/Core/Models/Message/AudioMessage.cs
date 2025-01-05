public class AudioMessage
{
    public Int16? SenderID { get; set; }
    public byte[] AudioData { get; set; }

    public AudioMessage(Int16? senderId, byte[] audioData)
    {
        SenderID = senderId;
        AudioData = audioData;
    }
}

