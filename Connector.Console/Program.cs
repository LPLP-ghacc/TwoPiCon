using Connector;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Program
{
    private static CConnector connector;

    public static async Task Main()
    {
        connector = new CConnector();

        //Console.WriteLine("Select Port:\n");

        int port = 5000;//Int32.Parse(Console.ReadLine());

        Console.WriteLine("Select connection type: 0 - Client; 1 - Server.\n");

        int ct = Int32.Parse(Console.ReadLine());

        CConnector.Tcp client = null;

        if(ct == 0)
        {
            Console.WriteLine("Connect to IP (write 'localhost' to connect 127.0.0.1):\n");

            string stringremoteIP = Console.ReadLine();

            IPAddress address = null;

            if(stringremoteIP == "localhost")
            {
                address = IPAddress.Loopback;
            }
            else
            {
                if(stringremoteIP != string.Empty || stringremoteIP != null)
                    address = IPAddress.Parse(stringremoteIP);
            }

            client = connector.TcpClient = new CConnector.Tcp(ConnectionType.Client, address, port, true);

            await client.ConnectToServer(address, port, true);

            client.SendToServer("Hello, World!".StringToByteArray());
        }
        else if(ct == 1)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Server public IPv4: " + CConnector.GetLocalIPv4(System.Net.NetworkInformation.NetworkInterfaceType.Ethernet));
            Console.WriteLine("Server local IPv4: " + CConnector.GetLocalIPAddress());
            Console.WriteLine("Server from socket IPv4: " + CConnector.GetLocalIPAddresFromSocket());
            Console.ForegroundColor= ConsoleColor.White;

            client = connector.TcpClient = new CConnector.Tcp(ConnectionType.Server, IPAddress.Any, port, true);
        }

        while (true)
        {
            if(ct == 0)
            {
                
            }
            if(ct == 1)
            {
                var bytes = await client.ServerReceive(ReceiveType.Bytes, 500, false);

                Console.WriteLine(bytes.ToArray().ByteArrayToUTF8String());
            }
        }
    }
}