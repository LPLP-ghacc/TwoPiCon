using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwoPiCon.Core.Abstract.Audio;

public class AudioClient
{
    private UdpClient _udpSender;
    private WaveInEvent _waveIn;
    private WaveOutEvent _waveOut;
    private BufferedWaveProvider _bufferedWaveProvider;
    private const int AudioPort = 5005;

    public AudioClient(string serverIp)
    {
        _udpSender = new UdpClient();
        _udpSender.Connect(IPAddress.Parse(serverIp), AudioPort);

        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(44100, 1)
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

    public void Start()
    {
        _waveIn.StartRecording();
        Task.Run(() => ReceiveAudio());
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
}