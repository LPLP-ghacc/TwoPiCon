using System.Text.Json.Serialization;

namespace TwoPiCon.Core.Abstract.Files;

public class FileMessage
{
    public FileMessage(string fileName, byte[] fileData)
    {
        FileName = fileName;
        FileData = fileData;
    }

    [JsonPropertyName("filename")]
    public string FileName { get; set; }
    [JsonPropertyName("filedata")]
    public byte[] FileData { get; set; }
}
