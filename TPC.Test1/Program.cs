using Resonance;
using Resonance.Adapters.Tcp;
using Resonance.Servers.Tcp;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Resonance.Transcoding.Json;

public static class Internet
{
    public static IPAddress? LocalIPAddress()
    {
        if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
        {
            return null;
        }

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        return host
            .AddressList
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public static string GetLocalIPAddresFromSocket()
    {
        string localIP;
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint.Address.ToString();
        }

        return localIP;
    }

    public static string GetLocalIPv4(NetworkInterfaceType _type)
    {
        string output = "";
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
            {
                IPInterfaceProperties adapterProperties = item.GetIPProperties();
                if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                {
                    foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                            break;
                        }
                    }
                }
            }
            if (output != "") { break; }
        }
        return output;
    }
}


public enum MessageType
{
    System,
    Text,
    Picture,
    Audio,
    Video,
    Gif,
    Default
}

/// <summary>
/// It is used to send messages to the server, to execute server commands.
/// </summary>
public class ServerMessage
{
    public required string Command { get; set; }
}

public interface IMessage
{
    public MessageType Type { get; set; }
    public ServerMessage? ServerMessage { get; set; }
    public Client? Sender { get; set; }
    public string Serialize();
}

public class RSAEncryption : IDisposable
{
    private RSA _rsa;
    private RSAParameters _privateKey;
    private RSAParameters _publicKey;

    public RSAEncryption(int keySize = 2048)
    {
        _rsa = RSA.Create(keySize);
        _privateKey = _rsa.ExportParameters(true);
        _publicKey = _rsa.ExportParameters(false);
    }

    public static byte[] Encrypt(string data, RSAParameters publicKey)
    {
        using (RSA rsaEncryptor = RSA.Create())
        {
            rsaEncryptor.ImportParameters(publicKey);
            return rsaEncryptor.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.Pkcs1);
        }
    }

    public string Decrypt(byte[] data)
    {
        try
        {
            lock (_rsa)
            {
                _rsa.ImportParameters(_privateKey);
                byte[] decryptedData = _rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(decryptedData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error decrypting message: {ex.Message}");
            throw;
        }
    }

    public RSAParameters GetPublicKey()
    {
        return _publicKey;
    }

    public void Dispose()
    {
        _rsa.Dispose();
    }
}

public class ClientsMessage : IMessage
{
    public ClientsMessage(List<(IResonanceTransporter trasporter, RSAParameters? param)> clients)
    {
        Type = MessageType.System;
        ServerMessage = null;
        Clients = clients;
        Sender = null;
    }

    public MessageType Type { get; set; }
    public List<(IResonanceTransporter trasporter, RSAParameters? param)> Clients;

    public ServerMessage? ServerMessage { get; set; }
    public Client? Sender { get; set; }

    public string Serialize()
    {
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();

        jsonSerializerOptions.WriteIndented = true;

        return JsonSerializer.Serialize(this, jsonSerializerOptions);
    }
}

public class KeyMessage : IMessage
{
    public KeyMessage(RSAParameters param, Client sender)
    {
        Type = MessageType.System;

        Parameters = param;

        ServerMessage = null;
        Sender = sender;
    }

    public MessageType Type { get; set; }
    public RSAParameters Parameters { get; set; }
    public ServerMessage? ServerMessage { get; set; }
    public Client Sender { get; set; }

    public string Serialize()
    {
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();

        jsonSerializerOptions.WriteIndented = true;

        return JsonSerializer.Serialize(this, jsonSerializerOptions);
    }
}

public class TextMessage
{
    public TextMessage(string content, Client sender, (IResonanceTransporter transporter, RSAParameters? param) receiver, ServerMessage? serverMessage = null)
    {
        Type = MessageType.Text;
        Receiver = receiver;

        if(Receiver.param == null)
        {
            Content = null;
        }
        else
        {
            RSAParameters parameters = (RSAParameters)Receiver.param;

            Content = !string.IsNullOrEmpty(content) ? GetContent(content, parameters) : Array.Empty<byte>();
        }
        
        Sender = sender;
        ServerMessage = serverMessage;
    }

    public byte[]? Content { get; private set; }
    public (IResonanceTransporter transporter, RSAParameters? param) Receiver { get; set; }
    public MessageType Type { get; set; }
    public ServerMessage? ServerMessage { get; set; }
    public Client? Sender { get; set; }

    private byte[] GetContent(string content, RSAParameters param)
    {
        return RSAEncryption.Encrypt(content, param);
    }

    public string Serialize()
    {
        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();

        jsonSerializerOptions.WriteIndented = true;

        return JsonSerializer.Serialize(this, jsonSerializerOptions);
    }
}

public class TestMessage
{
    public TestMessage()
    {
        Content = "Hello, World!";
    }

    public string Content { get; set; }
}

public class Client
{
    private IResonanceTransporter? _transporter;
    private List<(IResonanceTransporter trasporter, RSAParameters? param)> _connectedClients = new List<(IResonanceTransporter, RSAParameters?)>();

    public Client()
    {

    }

    public Action<TextMessage>? OnTextMessageReceived = null;

    private RSAEncryption RSAEncryption { get; set; } = new RSAEncryption();

    public async Task Connect(string remoteIp, int port)
    {
        Console.WriteLine("Trying to connect...");
        _transporter = await BuildConnect(remoteIp, port);

        if(_transporter != null)
            await _transporter.SendAsync(new TestMessage());
    }

    private async Task<IResonanceTransporter?> BuildConnect(string remoteIp, int port)
    {
        if (_transporter == null)
        {
            IResonanceTransporter transporter = new ResonanceTransporter();
            transporter.Adapter = new TcpAdapter(remoteIp, port);

            transporter.StateChanged += async (s, e) =>
            {
                if (_transporter == null)
                    return;

                if (transporter.State == ResonanceComponentState.Connected)
                {
                    Console.WriteLine($"Client: {typeof(Client).Name} Connected!");
                    await _transporter.SendAsync(new KeyMessage(RSAEncryption.GetPublicKey(), this));
                }
            };

            transporter.MessageReceived += (s, e) =>
            {
                if (e.Message.Object is IMessage message)
                {
                    if (message.Type == MessageType.System)
                    {
                        if (message is ClientsMessage clientsMessage)
                        {
                            _connectedClients = clientsMessage.Clients;
                        }
                    }

                    if (message.Type == MessageType.Text)
                    {
                        if (message is TextMessage textMessage)
                        {
                            if (textMessage.Content != null)
                                Console.WriteLine(RSAEncryption.Decrypt(textMessage.Content));
                        }
                    }
                }
            };

            try
            {
                await transporter.ConnectAsync();
                Console.WriteLine("Client: Connection successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
            }

            return transporter;
        }

        return null;
    }

    public async Task SendTextMessage(string text)
    {
        foreach (var client in _connectedClients)
        {
            if(client.param != null)
            {
                var param1 = client.param;

                var message = new TextMessage(text, this, client);

                if (_transporter != null)
                    await _transporter.SendAsync(message);
            }
        }
    }

    public RSAParameters GetPublicKey()
    {
        return RSAEncryption.GetPublicKey();
    }
}

public class Server
{
    private readonly List<(IResonanceTransporter trasporter, RSAParameters? param)> _connectedClients = new List<(IResonanceTransporter, RSAParameters?)>();

    public Server(int port)
    {
        ResonanceServer = new ResonanceTcpServer(port);
    }

    private ResonanceTcpServer ResonanceServer { get; set; }

    public async Task Start()
    {
        ResonanceServer.ConnectionRequest += async (s, e) =>
        {
            await GetTransporter(e);
        };

        Console.Clear();
        Console.WriteLine("Starting server...");

        await ResonanceServer.StartAsync();

        Console.Clear();
        Console.WriteLine("Server started.");
    }

    private async Task<IResonanceTransporter> GetTransporter(
        ResonanceListeningServerConnectionRequestEventArgs<TcpAdapter> e)
    {
        IResonanceTransporter transporter = new ResonanceTransporter();
        transporter.Adapter = e.Accept();

        transporter.MessageReceived += async (s, args) =>
        {
            Console.WriteLine($"Message received from {args.Transporter.Adapter}");

            if (args.Message.Object is IMessage message)
            {
                // Command handler
                if (message.ServerMessage != null)
                    CommandHandler(message.ServerMessage);

                // System message handler
                if(message.Type == MessageType.System)
                {
                    if (message is KeyMessage keyMessage)
                    {
                        _connectedClients.Add(new(args.Transporter, keyMessage.Parameters));

                        await transporter.SendAsync(new ClientsMessage(_connectedClients));

                        Console.WriteLine($"Server: handshake!");
                    }

                    return;
                }

                if(message.Type == MessageType.Text)
                {
                    if(message is TextMessage textMessage)
                    {
                        foreach (var client in _connectedClients)
                        {
                            // Broadcast message from sender to receiver
                            if (client.trasporter != textMessage.Sender)
                            {
                                if(client.trasporter == textMessage.Receiver.transporter)
                                {
                                    await client.trasporter.SendAsync(message);
                                }
                            }
                        }
                    }
                }
            }
        };

        transporter.StateChanged += async (s, args) =>
        {
            if (transporter.State == ResonanceComponentState.Connected)
            {
                Console.WriteLine("Transporter connected.");
            }
            else if (transporter.State == ResonanceComponentState.Disposed)
            {
                Console.WriteLine("Transporter disposed.");
            }
            else if (transporter.State == ResonanceComponentState.Failed)
            {
                Console.WriteLine("Transporter failed. Check network settings.");
            }
            else if (transporter.State == ResonanceComponentState.Disconnected)
            {
                Console.WriteLine("Transporter disconnected.");
            }
        };

        await transporter.ConnectAsync();
        Console.WriteLine("Server transporter connected.");

        return transporter;
    }

    private void CommandHandler(ServerMessage message)
    {
        var command = message.Command;
    }
}

public class Program
{
    public static async Task Main()
    {
         int port = 8888;
         
         Console.WriteLine("Select connection type: [1 - Server], [2 - Client]");
         
         int type = int.Parse(Console.ReadLine());
         
         if(type == 1)
         {
             Server server = new Server(port);
         
             await server.Start();
         
             Console.ForegroundColor = ConsoleColor.White;
             Console.Write("Server public IPv4: ");
             Console.ForegroundColor = ConsoleColor.Green;
             Console.WriteLine(Internet.GetLocalIPv4(NetworkInterfaceType.Ethernet));
             Console.ForegroundColor = ConsoleColor.White;
             Console.Write("Server local IPv4: ");
             Console.ForegroundColor = ConsoleColor.Green;
             Console.WriteLine(Internet.GetLocalIPAddress());
             Console.ForegroundColor = ConsoleColor.Gray;
         
             while (true)
             {
                 string text = Console.ReadLine();
             }
         }
         
         if(type == 2)
         {
             Client client = new Client();
         
             string clientWelcomeMessage = "/connect [address] - to connect to server."; 
         
             Console.WriteLine(clientWelcomeMessage);
         
             string cmd = Console.ReadLine();
         
             string ip = string.Empty;
         
             if (cmd.StartsWith("/"))
             {
                 if (cmd.Contains("/connect"))
                 {
                     ip = cmd.Substring("/connect ".Length);
                 }
             }
         
             if (!string.IsNullOrEmpty(ip))
             {
                 await client.Connect(ip, port);
             }
             else
             {
                 Console.WriteLine($"[{ip}] - Is Null Or Empty");
             }

             while (true)
             {
                 string text = Console.ReadLine();
         
                 await client.SendTextMessage(text);
             }
         }
    }
}