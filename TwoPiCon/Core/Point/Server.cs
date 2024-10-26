
using Resonance;
using Resonance.Adapters.Tcp;
using Resonance.Servers.Tcp;
using System.Net.NetworkInformation;
using TwoPiCon.Core.Abstract.Chat;
using TwoPiCon.Core;
using TwoPiCon.Core.Abstract.Files;
using TwoPiCon.Core.Abstract.Audio;

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
    private VoiceChatServer? AudioServer { get; set; }
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
            Console.Write(Internet.GetLocalIPv4(NetworkInterfaceType.Ethernet) + "\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Server local IPv4: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Internet.GetLocalIPAddress() + "\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        await _server.StartAsync();
        StartListeningVoiceChat();

        if (IsDebug)
            Console.WriteLine("Server started. Awaiting connections...");  
    }

    public void Stop()
    {
        _server.Stop();
        _server.Dispose();
    }

    public void StartListeningVoiceChat()
    {
        AudioServer = new VoiceChatServer(IsDebug);
        var listening = Task.Run(() => AudioServer.Start());
    }

    public void StopListeningVoiceChat()
    {
        if(AudioServer != null)
            AudioServer.Stop();
    }

    private async void ServerConnectionRequest(object? sender, ResonanceListeningServerConnectionRequestEventArgs<TcpAdapter> e)
    {
        var transporter = ResonanceTransporter.Builder
            .Create()
            .WithAdapter(e.Accept())
            .WithJsonTranscoding()
            .Build();

        _connectedClients.Add(transporter);
        transporter.MessageReceived += TransporterMessageReceived;
        transporter.StateChanged += (s, args) =>
        {
            if (transporter.State == ResonanceComponentState.Failed || transporter.State == ResonanceComponentState.Disconnected)
            {
                _connectedClients.Remove(transporter);
                if (IsDebug)
                    Console.WriteLine("Client disconnected.");
            }
        };

        await transporter.ConnectAsync();
        if (IsDebug)
            Console.WriteLine("Client connected.");
    }

    private async void TransporterMessageReceived(object? sender, ResonanceMessageReceivedEventArgs e)
    {
        if (e.Message.Object is TextMessage message)
        {
            if (IsDebug)
            {
                Console.WriteLine(e.Message.Object.ToString());
                Console.WriteLine($"Received message from client: {message.Content.Text}");
            }

            await BroadcastMessage(message);
        }
        else if (e.Message.Object is FileMessage fileMessage)
        {
            if (IsDebug)
                Console.WriteLine($"Received file from client: {fileMessage.FileName}");
            await BroadcastFile(fileMessage);
        }
        else
        {
            if (IsDebug)
                Console.WriteLine($"Received unknown message type.");
        }
    }

    private static async Task BroadcastFile(FileMessage fileMessage)
    {
        foreach (var client in _connectedClients)
        {
            await client.SendAsync(fileMessage);
        }
    }

    public async Task ReceiveFileAsync(FileMessage fileMessage)
    {
        string filePath = Path.Combine("ReceivedFiles", fileMessage.FileName);
        await File.WriteAllBytesAsync(filePath, fileMessage.FileData);
        if (IsDebug)
            Console.WriteLine($"File received and saved to {filePath}");
    }

    private static async Task BroadcastMessage(TextMessage message)
    {
        foreach (var client in _connectedClients)
        {
            //client.SendAsync(message).ContinueWith((task) =>
            //{
            //    if (task.IsFaulted)
            //    {
            //        Console.WriteLine($"Failed to send message to a client: {task.Exception?.GetBaseException().Message}");
            //    }
            //});

            await client.SendAsync(message);
        }
    }
}
