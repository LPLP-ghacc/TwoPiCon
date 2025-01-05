using static Server;

public record SyncMessage(List<ClientElement>? ConnectedClients)
{
    public List<ClientElement>? ConnectedClients { get; private set; } = ConnectedClients;
}

