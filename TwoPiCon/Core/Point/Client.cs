
using Resonance;
using TwoPiCon.Core.Abstract.Points;
using TwoPiCon.Utils;

namespace TwoPiCon.Core.Point;

public class Client : IPoint, IFileTranslator, IAudioTranslator
{
    private IResonanceTransporter? transporter;
    public Messenger? messenger;

    public Client(bool isDebug)
    {
        IsDebug = isDebug;
    }

    public Boolean IsDebug { get; set; }

    public async Task<bool> ConnectToAsync(String remoteIP, Int32 port)
    {
        bool isConnected = false;

        try
        {
            transporter = ResonanceTransporter.Builder
                .Create()
                .WithTcpAdapter()
                .WithAddress(remoteIP)
                .WithPort(port)
                .WithJsonTranscoding()
                .Build();

            isConnected = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        messenger = new Messenger(transporter);

        transporter.ConnectionLost += (s, e) => Console.WriteLine("Lost connection to the server.");

        transporter.StateChanged += (s, e) =>
        {
            Console.WriteLine($"Client transporter state changed: {transporter.State}");
        };

        transporter.MessageReceived += messenger.ReceiveMessage;

        await transporter.ConnectAsync();

        if (isConnected)
            Console.WriteLine("Connected to server. You can start sending messages.");

        return isConnected;
    }

    public void Dispose()
    {
        if (transporter != null)
            transporter.Dispose();
        Console.WriteLine("Disconnected from server.");
    }
}