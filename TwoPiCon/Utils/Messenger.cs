
using Resonance;
using TwoPiCon.Models.Chat;
using TwoPiCon.Core.Abstract.Messages;
using TwoPiCon.Core.Abstract.Points;

namespace TwoPiCon.Utils;

public class Messenger : IMessenger
{
    public Messenger(IResonanceTransporter transporter)
    {
        Transporter = transporter;
    }

    private IResonanceTransporter Transporter { get; set; }

    public void ReceiveMessage(object? sender, ResonanceMessageReceivedEventArgs e)
    {
        if (e.Message.Object is SampleTextMessage sMessage)
        {
            try
            {
                Console.WriteLine($"Received encrypted message: {sMessage.Content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decrypting message: {ex.Message}");
            }
        }
    }

    public void SendMessage(IMessage message)
    {
        try
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (Transporter != null)
                Transporter.SendAsync(message);

            Console.WriteLine($"Sent message: {message.Content}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public async Task SendMessageAsync(IMessage message)
    {
        try
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (Transporter != null)
                await Transporter.SendAsync(message);

            Console.WriteLine($"Sent message: {message.Content}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }
}
