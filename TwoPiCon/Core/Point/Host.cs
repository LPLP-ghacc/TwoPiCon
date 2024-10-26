
using NAudio.CoreAudioApi;
using Resonance;
using Resonance.Servers.Tcp;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.JavaScript;
using TwoPiCon.Core.Abstract.Audio;

namespace TwoPiCon.Core.Point;

public class Host
{
    private Server _server;
    public Client host;

    /// <summary>
    /// 
    /// </summary>
    public Host(string remoteIP, Int32 port, Boolean isDebug = true)
    {
        RemoteIP = remoteIP;
        Port = port;
        IsDebug = isDebug;
    }

    private string RemoteIP { get; set; }
    private int Port { get; set; }
    private VoiceChatServer AudioServer { get; set; }
    private VoiceChatClient? AudioClient { get; set; }
    private bool IsDebug { get; set; }

    public async Task StartAsync()
    {
        _server = new Server(Port, IsDebug);
        await _server.Start();

        host = new Client(IsDebug);
        await host.ConnectToAsync(RemoteIP, Port);
    }

    public void Stop()
    {
        host.Disconnect();
        host.Dispose();
        _server.Stop();
    }

    public async Task StartListeningVoiceChatAsync()
    {
        AudioServer = new VoiceChatServer(IsDebug);
        await Task.Run(() => AudioServer.Start());
    }

    public void StopListeningVoiceChat()
    {
        if (AudioServer != null)
            AudioServer.Stop();
    }

    public async Task StartAudioTransmissionAsync()
    {
        AudioClient = new VoiceChatClient(RemoteIP, VoiceChatQualityType.Default);

        await Task.Run(AudioClient.Start);
    }

    public void StopAudioTransmission()
    {
        if (AudioClient != null)
            AudioClient.Stop();
    }

    public static string GetServerIP()
    {
        return Internet.GetLocalIPv4(NetworkInterfaceType.Ethernet);
    }
}
