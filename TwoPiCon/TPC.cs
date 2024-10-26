using Resonance.Transcoding.Json;
using Resonance.WebRTC;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using TwoPiCon.Core;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using System.Reflection;
using NAudio.Wave;
using System.Diagnostics;
using TwoPiCon.Core.Point;

namespace TwoPiCon;

public class TPC
{
    public async Task<Server> GetServerAsync(Int32 port, bool isDebug)
    {
        Server = new Server(port, isDebug);

        await Server.Start();

        return Server;
    }

    public Server? Server { get; private set; }

    public Client? Client { get; private set; }
}