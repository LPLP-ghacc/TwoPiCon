using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TwoPiCon.Models.Chat;

namespace TwoPiCon.Models.Text;

public class ServerCMD
{
    /// <summary>
    /// the system command
    /// </summary>
    [JsonPropertyName("command")]
    public string Command { get; set; }
    [JsonPropertyName("author")]
    public Author Author { get; set; }
    [JsonPropertyName("execution_time")]
    public DateTime ExecutionTime { get; set; }
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
