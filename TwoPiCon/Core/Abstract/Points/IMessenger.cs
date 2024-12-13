using Resonance;
using TwoPiCon.Core.Abstract.Messages;

namespace TwoPiCon.Core.Abstract.Points;

public interface IMessenger
{
    public void SendMessage(IMessage message);
    public void ReceiveMessage(object? sender, ResonanceMessageReceivedEventArgs e);
    public Task SendMessageAsync(IMessage message);
}
