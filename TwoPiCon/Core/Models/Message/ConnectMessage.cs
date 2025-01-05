using static Server;

public record ConnectMessage(List<ClientElement>? ConnectedClients, ServerMessage? ServerMessage, Int16? clientID)
{
    public Int16? ClientID { get; private set; } = clientID;
    public List<ClientElement>? ConnectedClients { get; private set; } = ConnectedClients;
    public ServerMessage? ServerMessage { get; private set; } = ServerMessage;
}

