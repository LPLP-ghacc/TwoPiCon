using System.Text.Json.Serialization;

namespace TwoPiCon.Models.Chat;

public class Content
{
    public Content(string text, List<string> urls)
    {
        Text = text;
        Urls = urls;
    }

    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("urls")]
    public List<string> Urls { get; set; }
}

