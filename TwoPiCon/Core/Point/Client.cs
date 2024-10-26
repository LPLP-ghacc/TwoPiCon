
using Resonance;
using TwoPiCon.Core.Abstract.Chat;
using TwoPiCon.Core.Abstract.Files;
using TwoPiCon.Core.Abstract.Audio;

namespace TwoPiCon.Core.Point;

public class Client
{
    private IResonanceTransporter? transporter;
    private VoiceChatClient? AudioClient;
    private string remoteIP = string.Empty;

    public Client(bool isDebug)
    {
        IsDebug = isDebug;
    }

    private bool IsDebug { get; set; }

    public async Task ConnectToAsync(string remoteIP, int port)
    {
        transporter = ResonanceTransporter.Builder
            .Create()
            .WithTcpAdapter()
            .WithAddress(remoteIP)
            .WithPort(port)
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
        };

        await transporter.ConnectAsync();

        this.remoteIP = remoteIP;

        Console.WriteLine("Connected to server. You can start sending messages.");
    }

    public void Disconnect()
    {
        if (transporter != null)
        {
            StopAudioTransmission();

            transporter?.Disconnect();
            transporter = null;

            if (IsDebug)
                Console.WriteLine("Disconnected from Server.");
        }
    }

    public void StartAudioTransmission()
    {
        AudioClient = new VoiceChatClient(remoteIP, VoiceChatQualityType.Default);

        var listeining = Task.Run(AudioClient.Start);
    }

    public void StopAudioTransmission()
    {
        if (AudioClient != null)
            AudioClient.Stop();
    }

    public async Task SendMessageAsync(string text)
    {
        try
        {
            Random random = new Random();
            var message = new TextMessage(random.Next(int.MaxValue), new Author("Anon", "none", "127.0.0.1"), new Content(text, new List<string>()), DateTime.Now);

            if(transporter != null)
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
            if (transporter != null)
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
        if (transporter != null)
            transporter.Dispose();
        Console.WriteLine("Disconnected from server.");
    }
}
