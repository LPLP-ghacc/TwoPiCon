
/// <summary>
/// It is used to send messages to the server, to execute server commands.
/// </summary>
public class ServerMessage
{
    public ServerMessage() { }

    public ServerMessage(string command)
    {
        Command = command;
    }

    public string Command { get; set; }
}

