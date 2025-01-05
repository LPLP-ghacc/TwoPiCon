public class TextMessage
{
    public TextMessage(short? clientID, byte[]? content)
    {
        ClientID = clientID;
        Content = content;
    }

    public Int16? ClientID { get; private set; }
    public int ContentType { get; private set; } = 1;
    public byte[]? Content { get; private set; }
    public string EncryptedText { get; set; }
}

