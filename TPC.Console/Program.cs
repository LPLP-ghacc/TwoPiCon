
using Resonance.Adapters.Tcp;
using Resonance;
using TwoPiCon;
using Resonance.Servers.Tcp;
using Connector;
using TwoPiCon.Core.Point;
using TPC.Console;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using TwoPiCon.Models.Chat;

public class Program
{
    public static void TestRSAEndToEnd()
    {
        using (RSA rsa = RSA.Create(2048))
        {
            RSAParameters publicKey = rsa.ExportParameters(false);
            RSAParameters privateKey = rsa.ExportParameters(true);

            string originalMessage = "Hello RSA!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(originalMessage);

            using (RSA rsaEncrypt = RSA.Create())
            {
                rsaEncrypt.ImportParameters(publicKey);
                byte[] encryptedBytes = rsaEncrypt.Encrypt(messageBytes, RSAEncryptionPadding.OaepSHA256);

                using (RSA rsaDecrypt = RSA.Create())
                {
                    rsaDecrypt.ImportParameters(privateKey);
                    byte[] decryptedBytes = rsaDecrypt.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
                    string decryptedMessage = Encoding.UTF8.GetString(decryptedBytes);

                    Console.WriteLine("Decrypted message: " + decryptedMessage);

                    Debug.Assert(decryptedMessage == originalMessage);
                }
            }
        }
    }

    public static async Task Main()
    {
        TestRSAEndToEnd();

        TwoPiCon.TPC tPC = new TwoPiCon.TPC();

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
            string connectCmd = "connect";

            var client = new Client(true);

            if (InstanceEngine.GetInstances().Count == 0)
            {
                Console.WriteLine("Connect to IP:\n");

                string stringremoteIP = Console.ReadLine();

                bool isConnected = await client.ConnectToAsync(stringremoteIP, port);

                if (isConnected)
                {
                    Instance ins = new Instance(stringremoteIP, port, DateTime.Now);
                    await ins.SaveAsync();
                }
            }
            else
            {
                Console.WriteLine($"you can connect to the new server using the \"{connectCmd} [ADDRESS]\" command.\nOR");
                Console.WriteLine("select the number of the saved instance:");

                var instances = InstanceEngine.GetInstances();

                for(int i = 0; i < instances.Count; i++)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"[{i}] : ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{instances[i].IPAddress}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($" - last update: {instances[i].LastUpdate}\n");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                var command = Console.ReadLine();

                string address = string.Empty;

                if (command.StartsWith(connectCmd))
                {
                    address = command.Substring(connectCmd.Length + 1);
                }
                else
                {
                    int number = int.Parse(Console.ReadLine());

                    var instance = instances[number];
                }

                bool isConnected = await client.ConnectToAsync(address, port);

                if (isConnected)
                {
                    Instance ins = new Instance(address, port, DateTime.Now);
                    await ins.SaveAsync();
                }
            }

            while (true)
            {
                string? text = Console.ReadLine();

                if (text == null)
                    return;

                await client.messenger.SendMessageAsync(new SampleTextMessage(text));
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
                    //host.host.StopAudioTransmission();
                }

                await host.host.messenger.SendMessageAsync(new SampleTextMessage(text));
            }
        }
    }
}
