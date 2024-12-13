
using Resonance;
using Resonance.Adapters.Tcp;
using Resonance.Servers.Tcp;
using System.Net.NetworkInformation;
using TwoPiCon.Models.Chat;

namespace TwoPiCon.Core.Point;

public class Server
{
    private ResonanceTcpServer _server;
    private static List<IResonanceTransporter> _connectedClients = new List<IResonanceTransporter>();

    public Server(int port, bool isDebug)
    {
        Port = port;
        IsDebug = isDebug;
    }

    private int Port { get; set; }
    private bool IsDebug { get; set; }

    public async Task Start()
    {
        _server = new ResonanceTcpServer(Port);
        _server.ConnectionRequest += ServerConnectionRequest;

        if (IsDebug)
        {
            Console.Clear();
            Console.WriteLine("Starting server...");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Server public IPv4: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Internet.GetLocalIPv4(NetworkInterfaceType.Ethernet));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Server local IPv4: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Internet.GetLocalIPAddress());
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        await _server.StartAsync();

        if (IsDebug)
            Console.WriteLine("Server started. Awaiting connections...");
    }

    private async void ServerConnectionRequest(object? sender, ResonanceListeningServerConnectionRequestEventArgs<TcpAdapter> e)
    {
        var transporter = ResonanceTransporter.Builder
            .Create()
            .WithAdapter(e.Accept())
            .WithJsonTranscoding()
            .Build();

        transporter.MessageReceived += TransporterMessageReceived;

        transporter.StateChanged += async (s, args) =>
        {
            if (transporter.State == ResonanceComponentState.Connected)
            {

            }
        };

        await transporter.ConnectAsync();
        _connectedClients.Add(transporter);

        if (IsDebug)
            Console.WriteLine("Client connected.");
    }

    private async void TransporterMessageReceived(object? sender, ResonanceMessageReceivedEventArgs e)
    {
        if (e.Message.Object is SampleTextMessage sMessage)
        {
            if (IsDebug)
            {
                Console.WriteLine($"Received message from client: {sMessage.Content}");
            }
            await BroadcastSMessage(sMessage);
        }
    }

    private static async Task BroadcastSMessage(SampleTextMessage message)
    {
        foreach (var client in _connectedClients)
        {
            await client.SendAsync(message);
        }
    }
}