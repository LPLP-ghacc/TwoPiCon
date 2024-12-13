using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace TwoPiCon.Models.Chat;

public class TextMessage
{
    public TextMessage(int id, Author author, Content content, DateTime createdAt)
    {
        Id = id;
        Author = author;
        Content = content;
        CreatedAt = createdAt;
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("author")]
    public Author Author { get; set; }
    [JsonPropertyName("content")]
    public Content Content { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

