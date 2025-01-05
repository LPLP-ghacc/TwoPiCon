using Resonance;
using Resonance.Adapters.Tcp;
using Resonance.Servers.Tcp;
using System.Security.Cryptography;

public class Server : IPoint
{
    private List<ClientElement> _connectedClients = [];

    public Server(int port)
    {
        ResonanceServer = new ResonanceTcpServer(port);

        Settings = new PointSettings();
        Type = PointType.Server;
        L = new L(this);

    }

    private ResonanceTcpServer ResonanceServer { get; set; }
    public PointType Type { get; set; }
    public L L { get; set; }
    public PointSettings Settings { get; set; }
    public Action<object> ActionMessageSender;

    public async Task Start()
    {
        ResonanceServer.ConnectionRequest += async (s, e) =>
        {
            await GetTransporter(e);
        };

        Console.Clear();
        L.M("Starting server...");

        await ResonanceServer.StartAsync();

        Console.Clear();
        L.M("Server started.");
    }

    private async Task<IResonanceTransporter> GetTransporter(
        ResonanceListeningServerConnectionRequestEventArgs<TcpAdapter> e)
    {
        IResonanceTransporter transporter = new ResonanceTransporter();
        transporter.Adapter = e.Accept();

        transporter.MessageReceived += async (s, args) =>
        {
            L.M($"Message received from {args.Transporter.Adapter}");

            if (args.Message.Object is KeyMessage keyMessage)
            {
                Random random = new Random();
                Int16 clientID = (short)random.Next(Int16.MaxValue);

                ClientElement element = new ClientElement(clientID, args.Transporter, keyMessage.Parameters);
                _connectedClients.Add(element);
                await transporter.SendAsync(new ConnectMessage(_connectedClients, new ServerMessage(), clientID));

                await SyncClients();

                L.M($"Server: handshake!");
            }

            if (args.Message.Object is TextMessage textMessage)
            {
                foreach (var client in _connectedClients)
                {
                    await client.Transporter.SendAsync(textMessage);
                }
            }

            ActionMessageSender?.Invoke(args.Message.Object);
        };

        transporter.StateChanged += async (s, args) =>
        {
            if (transporter.State == ResonanceComponentState.Connected)
            {
                L.M("Transporter connected.");
            }
            else if (transporter.State == ResonanceComponentState.Disposed)
            {
                L.M("Transporter disposed.");
            }
            else if (transporter.State == ResonanceComponentState.Failed)
            {
                L.M("Transporter failed. Check network settings.");
            }
            else if (transporter.State == ResonanceComponentState.Disconnected)
            {
                L.M("Transporter disconnected.");
            }
        };

        await transporter.ConnectAsync();
        L.M("Server transporter connected.");

        return transporter;
    }

    private void CommandHandler(ServerMessage message)
    {
        var command = message.Command;
    }

    private async Task SyncClients()
    {
        SyncMessage syncMessage = new SyncMessage(_connectedClients);

        foreach (var client in _connectedClients)
        {
            if (client.Transporter != null)
            {
                await client.Transporter.SendAsync(syncMessage);
            }
        }
    }

    public List<ClientElement> GetConnectedClients()
    {
        return _connectedClients;
    }

    public record ClientElement(Int16 ClientID, IResonanceTransporter? Transporter, RSAParameters Parameters)
    {
        public Int16 ClientID { get; private set; } = ClientID;
        public IResonanceTransporter? Transporter { get; private set; } = Transporter;
        public RSAParameters Parameters { get; private set; } = Parameters;
    }
}

