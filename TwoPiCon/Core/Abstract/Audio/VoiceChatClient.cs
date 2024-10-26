using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwoPiCon.Core.Abstract.Audio;

public enum VoiceChatQualityType
{
    Default,
    Ultra,
    High,
    Meduim,
    Low,
    UltraStereo,
    HighStereo,
    MediumStereo,
    LowStereo,
}

public static class VoiceChatQuality
{
    public static WaveFormat Default = new WaveFormat();
    public static WaveFormat Ultra = new WaveFormat(48000, 1); 
    public static WaveFormat High = new WaveFormat(44100, 1);
    public static WaveFormat Medium = new WaveFormat(44100, 1);
    public static WaveFormat Low = new WaveFormat(22050, 1);
    public static WaveFormat UltraStereo = new WaveFormat(48000, 2);
    public static WaveFormat HighStereo = new WaveFormat(44100, 2);
    public static WaveFormat MediumStereo = new WaveFormat(44100, 2);
    public static WaveFormat LowStereo = new WaveFormat(22050, 2);
}

public class VoiceChatClient
{
    private static CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
    private CancellationToken token;
    private UdpClient _udpSender;
    private WaveInEvent _waveIn;
    private WaveOutEvent _waveOut;
    private BufferedWaveProvider _bufferedWaveProvider;
    private const int AudioPort = 5005;

    public VoiceChatClient(string serverIp, VoiceChatQualityType qualityType)
    {
        _udpSender = new UdpClient();
        _udpSender.Connect(IPAddress.Parse(serverIp), AudioPort);

        var quality = GetWaveFormatByQuality(qualityType);

        _waveIn = new WaveInEvent
        {
            WaveFormat = quality
        };

        _bufferedWaveProvider = new BufferedWaveProvider(_waveIn.WaveFormat);
        _waveOut = new WaveOutEvent();
        _waveOut.Init(_bufferedWaveProvider);

        _waveIn.DataAvailable += (s, e) =>
        {
            _udpSender.Send(e.Buffer, e.BytesRecorded);
        };

        _waveOut.Play();
    }

    public async Task Start()
    {
        if(cancelTokenSource == null)
            cancelTokenSource = new CancellationTokenSource();

        token = cancelTokenSource.Token;

        _waveIn.StartRecording();
        await Task.Run(ReceiveAudio, token);
    }

    public void Stop() 
    {
        _waveIn.StopRecording();
        cancelTokenSource.Cancel();
        cancelTokenSource.Dispose();
    }

    private void ReceiveAudio()
    {
        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, AudioPort);
        while (true)
        {
            byte[] receivedBytes = _udpSender.Receive(ref serverEndPoint);
            _bufferedWaveProvider.AddSamples(receivedBytes, 0, receivedBytes.Length);
        }
    }

    private WaveFormat GetWaveFormatByQuality(VoiceChatQualityType qualityType)
    {
        switch (qualityType)
        {
            case VoiceChatQualityType.Default:
                return VoiceChatQuality.Default;
            case VoiceChatQualityType.Ultra:
                return VoiceChatQuality.Ultra;
            case VoiceChatQualityType.High:
                return VoiceChatQuality.High;
            case VoiceChatQualityType.Meduim:
                return VoiceChatQuality.Medium;
            case VoiceChatQualityType.Low:
                return VoiceChatQuality.Low;
            case VoiceChatQualityType.UltraStereo:
                return VoiceChatQuality.UltraStereo;
            case VoiceChatQualityType.HighStereo:
                return VoiceChatQuality.HighStereo;
            case VoiceChatQualityType.MediumStereo:
                return VoiceChatQuality.MediumStereo;
            case VoiceChatQualityType.LowStereo:
                return VoiceChatQuality.LowStereo;
            default:
                return VoiceChatQuality.Default;
        }
    }
}