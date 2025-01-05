using System.Net.NetworkInformation;

public class Program
{
    public static async Task Main()
    {
        int port = 8888;

        Console.WriteLine("Select connection type: [1 - Server], [2 - Client]");

        int type = int.Parse(Console.ReadLine());

        if (type == 1)
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

        if (type == 2)
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

                if (cmd.Contains("/sa"))
                {
                    client.Audio.StartAudioCapture();
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
                string? input = await Task.Run(() => Console.ReadLine());

                if (!string.IsNullOrEmpty(input))
                {
                    if (input.StartsWith("/"))
                    {
                        if (input.Contains("/sa"))
                        {
                            Task.Run(() => client.Audio.StartAudioCapture());
                        }
                    }
                    else
                    {
                        await client.SendTextMessage(input);
                    }
                }
            }
        }
    }
}

