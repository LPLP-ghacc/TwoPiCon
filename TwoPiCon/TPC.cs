
using Resonance;
using Resonance.Adapters.Tcp;
using Resonance.Servers.Tcp;
using Resonance.Transcoding.Json;
using Resonance.WebRTC;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using TwoPiCon.Core.Abstract.Chat;
using TwoPiCon.Core;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using TwoPiCon.Core.Abstract.Files;
using TwoPiCon.Core.Abstract.Audio;

namespace TwoPiCon;

public class Server
{
    private static List<IResonanceTransporter> _connectedClients = new List<IResonanceTransporter>();
    private static ObservableCollection<TextMessage> Messages = new ObservableCollection<TextMessage>();

    public Server(Int32 port)
    {
        Port = port;
        AudioEndpoint = new AudioEndpoint();
    }

    private Int32 Port { get; set; }
    private AudioEndpoint AudioEndpoint { get; set; }

    public async Task Init()
    {
        ResonanceTcpServer server = new ResonanceTcpServer(Port);
        server.ConnectionRequest += ServerConnectionRequest;
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

        await server.StartAsync();
        AudioEndpoint.Init();
        Console.WriteLine("Server started. Awaiting connections...");
        Console.Title = "Server";

        Console.ReadLine();
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
                Console.WriteLine("Client disconnected.");
            }
        };

        await transporter.ConnectAsync();
        Console.WriteLine("Client connected.");
    }

    private async void TransporterMessageReceived(object? sender, ResonanceMessageReceivedEventArgs e)
    {
        if (e.Message.Object is TextMessage message)
        {
            Messages.Add(message);
            Console.WriteLine(e.Message.Object.ToString());

            Console.WriteLine($"Received message from client: {message.Content.Text}");
            await BroadcastMessage(message);
        }
        else if (e.Message.Object is FileMessage fileMessage)
        {
            Console.WriteLine($"Received file from client: {fileMessage.FileName}");
            await BroadcastFile(fileMessage);
        }
        else if (e.Message.Object is AudioMessage audioMessage)
        {
            //AudioEndpoint.waveProvider.AddSamples(audioMessage.AudioData, 0, audioMessage.ByteCount);
            await BroadcastAudioStream(audioMessage);
        }
        else
        {
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

    private static async Task BroadcastAudioStream(AudioMessage audioMessage)
    {
        foreach (var client in _connectedClients)
        {
            await client.SendAsync(audioMessage);
        }
    }

    public async Task ReceiveFileAsync(FileMessage fileMessage)
    {
        string filePath = Path.Combine("ReceivedFiles", fileMessage.FileName);
        await File.WriteAllBytesAsync(filePath, fileMessage.FileData);
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

public class Client
{
    private IResonanceTransporter transporter;

    public Client(String remoteIP, Int32 port)
    {
        RemoteIP = remoteIP;
        Port = port;

        AudioEndpoint = new AudioEndpoint();
    }

    private String RemoteIP { get; set; }
    private Int32 Port { get; set; }
    private AudioEndpoint AudioEndpoint { get; set; }

    public async Task Init()
    {
        transporter = ResonanceTransporter.Builder
            .Create()
            .WithTcpAdapter()
            .WithAddress(RemoteIP)
            .WithPort(Port)
            .WithJsonTranscoding()
            .Build();

        transporter.ConnectionLost += (s, e) => Console.WriteLine("Lost connection to the server.");

        transporter.StateChanged += (s, e) =>
        {
            Console.WriteLine($"Client transporter state changed: {transporter.State}");
        };

        transporter.MessageReceived += (sender, e) =>
        {
            if (e.Message.Object is TextMessage message)
            {
                Console.WriteLine($"Received broadcast: {message.Content.Text}");
            }
            else if (e.Message.Object is AudioMessage audioMessage)
            {
                AudioEndpoint.waveProvider.AddSamples(audioMessage.AudioData, 0, audioMessage.ByteCount);
            }
        };

        AudioEndpoint.Init();
        await transporter.ConnectAsync();

        Console.Title = "Client";

        Console.Clear();
        Console.WriteLine("Connected to server. You can start sending messages.");
    }

    public void StartAudioTransmission()
    {
        AudioEndpoint.StartAudioTransmission(transporter);
    }

    public void StopAudioTransmission()
    {
        AudioEndpoint.StopAudioTransmission();
    }

    public async Task SendMessageAsync(string text)
    {
        try
        {
            Random random = new Random();
            var message = new TextMessage(random.Next(int.MaxValue), new Author("Anon", "none", "127.0.0.1"), new Content(text, new List<string>()), DateTime.Now);
            await transporter.SendAsync(message);
            Console.WriteLine($"Sent message: {text}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public async Task SendFileAsync(string filePath)
    {
        try
        {
            byte[] fileData = await File.ReadAllBytesAsync(filePath);
            FileMessage fileMessage = new FileMessage(Path.GetFileName(filePath), fileData);
            await transporter.SendAsync(fileMessage);
            Console.WriteLine($"Sent file: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending file: {ex.Message}");
        }
    }

    public void Dispose()
    {
        transporter.Dispose();
        Console.WriteLine("Disconnected from server.");
    }
}

public class TPC
{
    public async Task<Server> GetServerAsync(Int32 port)
    {
        Server = new Server(port);

        await Server.Init();

        return Server;
    }

    public async Task<Client> GetClientAsync(String remoteIP, Int32 port)
    {
        Client = new Client(remoteIP, port);

        await Client.Init();

        return Client;
    }

    public Server? Server { get; private set; }

    public Client? Client { get; private set; }
}