
using NAudio.CoreAudioApi;
using Resonance;
using Resonance.Servers.Tcp;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.JavaScript;
using TwoPiCon.Models.Audio;

namespace TwoPiCon.Core.Point;

public class Host
{
    private Server _server;
    public Client host;

    public Host(string remoteIP, Int32 port, Boolean isDebug = true)
    {
        RemoteIP = remoteIP;
        Port = port;
        IsDebug = isDebug;
    }

    private string RemoteIP { get; set; }
    private int Port { get; set; }
    private VoiceChatServer? AudioServer { get; set; }
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
        //host?.Disconnect();
        host?.Dispose();
        //_server?.Stop();
    }

    public void StartListeningVoiceChatAsync()
    {
        if (AudioServer == null)
        {
            AudioServer = new VoiceChatServer(IsDebug);
            _ = Task.Run(() => AudioServer.Start()); // Запуск задачи вне основного потока и без await.
        }
    }

    public void StopListeningVoiceChat()
    {
        AudioServer?.Stop();
    }

    public async Task StartAudioTransmissionAsync()
    {
        if (AudioClient == null)
        {
            AudioClient = new VoiceChatClient(RemoteIP, VoiceChatQualityType.Default);
            await Task.Run(AudioClient.Start);
        }
    }

    public void StopAudioTransmission()
    {
        AudioClient?.Stop();
    }

    public static string GetServerIP()
    {
        return Internet.GetLocalIPv4(NetworkInterfaceType.Ethernet);
    }
}