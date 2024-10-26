
using Resonance.Adapters.Tcp;
using Resonance;
using TwoPiCon;
using Resonance.Servers.Tcp;
using Connector;
using TwoPiCon.Core.Point;

public class Program
{
    public static async Task Main()
    {
        TPC tPC = new TPC();

        int port = 5000;

        Console.WriteLine("Select TwoPiCon type: 0 - Client; 1 - Server; 2 - Host;\n");

        int ct = Int32.Parse(Console.ReadLine());

        if(ct == 1)
        {      
            var server = await tPC.GetServerAsync(port, true);
            Console.ReadKey();
        }
        else if(ct == 0)
        {    
            Console.WriteLine("Connect to IP:\n");

            string stringremoteIP = Console.ReadLine();

            var client = new Client(true);
            await client.ConnectToAsync(stringremoteIP, port);

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
        else if(ct == 2)
        {
            Host host = new Host(Host.GetServerIP(), port);

            await host.StartAsync();

            while (true)
            {
                string? text = Console.ReadLine();

                if (text == null)
                    return;

                if (text == "/sa")
                {
                    await host.StartAudioTransmissionAsync();
                }
                if (text == "/ss")
                {
                    host.host.StopAudioTransmission();
                }

                await host.host.SendMessageAsync(text);
            }
        }
    }
}
