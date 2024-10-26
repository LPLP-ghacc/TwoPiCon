using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwoPiCon.Core.Abstract.Audio;

public class VoiceChatServer
{
    private UdpClient _udpListener;
    private const int AudioPort = 5005;
    private List<IPEndPoint> _clientEndPoints = new List<IPEndPoint>();

    public VoiceChatServer(Boolean isdebug)
    {
        IsListening = true;
        _udpListener = new UdpClient(AudioPort);
        IsDebug = isdebug;
    }

    private bool IsDebug { get; set; }
    public bool IsListening { get; set; }

    public void Start()
    {
        if(IsDebug)
            Console.WriteLine("Audio Server Listening...");
        while (IsListening)
        {
            try
            {
                IPEndPoint receivedEndPoint = new IPEndPoint(IPAddress.Any, AudioPort);
                byte[] receivedData = _udpListener.Receive(ref receivedEndPoint);

                // Add client if not already known
                if (!_clientEndPoints.Contains(receivedEndPoint))
                {
                    _clientEndPoints.Add(receivedEndPoint);
                    if (IsDebug)
                        Console.WriteLine($"Added new client from {receivedEndPoint.Address}:{receivedEndPoint.Port}");
                }

                // Broadcast to all known clients except sender
                foreach (var clientEndPoint in _clientEndPoints)
                {
                    if (!receivedEndPoint.Equals(clientEndPoint))
                    {
                        _udpListener.Send(receivedData, receivedData.Length, clientEndPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public void Stop()
    {
        IsListening = false;
    }
}
