using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TwoPiCon.Core.Abstract.Messages;

namespace TwoPiCon.Models.Chat;

public class SampleTextMessage : IMessage
{
    public SampleTextMessage(string content)
    {
        Content = content;
    }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}
