using System.Text.Json.Serialization;

namespace TwoPiCon.Core.Abstract.Chat;

public class Author
{
    public Author(string name, string description, string authorIP)
    {
        Name = name;
        Description = description;
        AuthorIP = authorIP;
    }

    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("author_ip")]
    public string AuthorIP { get; set; }
}

