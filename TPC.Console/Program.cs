
using Resonance.Adapters.Tcp;
using Resonance;
using TwoPiCon;
using Resonance.Servers.Tcp;
using Connector;

public class Program
{
    public static async Task Main()
    {
        TPC tPC = new TPC();

        int port = 5000;

        Console.WriteLine("Select TwoPiCon type: 0 - Client; 1 - Server.\n");

        int ct = Int32.Parse(Console.ReadLine());

        if(ct == 1)
        {      
            var server = await tPC.GetServerAsync(port);
        }
        else if(ct == 0)
        {
            
            Console.WriteLine("Connect to IP:\n");

            string stringremoteIP = Console.ReadLine();

            var client = await tPC.GetClientAsync(stringremoteIP, port);

            while (true)
            {
                string? text = Console.ReadLine();

                if (text == null)
                    return;

                if(text == "/sa")
                {
                    client.StartAudioTransmission();
                }
                if(text == "/ss")
                {
                    client.StopAudioTransmission();
                }

                await client.SendMessageAsync(text);
            }
        }
    }
}
