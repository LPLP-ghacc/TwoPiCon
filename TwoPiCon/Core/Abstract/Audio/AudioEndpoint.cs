
using Resonance;
using NAudio.Wave;

namespace TwoPiCon.Core.Abstract.Audio;

public class AudioEndpoint
{
    public WaveInEvent waveIn;
    public WaveOutEvent waveOut;
    public BufferedWaveProvider waveProvider;
    public bool isTransmittingAudio = false;

    public void Init()
    {
        waveOut = new WaveOutEvent();
        waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1));
        waveOut.Init(waveProvider);
        waveOut.Play();
    }

    public void StartAudioTransmission(IResonanceTransporter transporter)
    {
        waveIn = new WaveInEvent();
        waveIn.WaveFormat = new WaveFormat(44100, 1);
        waveIn.DataAvailable += async (sender, args) =>
        {
            if (isTransmittingAudio)
            {
                AudioMessage audioMessage = new AudioMessage(args.Buffer, args.BytesRecorded);
                await transporter.SendAsync(audioMessage);
            }
        };
        waveIn.StartRecording();
        isTransmittingAudio = true;
    }

    public void StopAudioTransmission()
    {
        isTransmittingAudio = false;
        waveIn?.StopRecording();
    }
}
