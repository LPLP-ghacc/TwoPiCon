using NAudio.Wave;
using Resonance;
using Resonance.Adapters.Tcp;
using System.Security.Cryptography;
using static Server;

public class AudioClient
{
    private WaveInEvent? _waveIn;
    private WaveOutEvent? _waveOut;
    private BufferedWaveProvider? _bufferedWaveProvider;

    public AudioClient(IPoint point)
    {
        IsEnableLog = point.Settings.IsEnableLog;

        if (point is Client Client)
        {
            Point = Client;
            Transporter = Client.GetTransporter();

            Action<object> onAudioMessageReceive = (object s) =>
            {
                if (s is AudioMessage audio)
                {
                    _bufferedWaveProvider?.AddSamples(audio.AudioData, 0, audio.AudioData.Length);
                }
            };

            Client.ActionMessageSender += onAudioMessageReceive;     
        }
        if(point is Server Server)
        {
            Point = Server;

            Action<object> onAudioMessageReceive = async (object s) =>
            {
                if (s is AudioMessage audioMessage)
                {
                    point.L.M($"AudioMessage received from Client [{audioMessage.SenderID}]");

                    foreach (var client in Server.GetConnectedClients())
                    {
                        if (client.ClientID != audioMessage.SenderID)
                        {
                            await client.Transporter.SendAsync(audioMessage);
                        }
                    }
                }
            };

            Server.ActionMessageSender += onAudioMessageReceive;
        }

        point.L.M("Audio client is On");
    }

    private IPoint Point { get; set; }
    private IResonanceTransporter Transporter { get; set; }
    private bool IsEnableLog { get; set; }

    public void StartAudioCapture()
    {
        Client? client = Point as Client;

        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(48000, 16, 1)
        };

        _waveIn.DataAvailable += async (s, e) =>
        {
            if (Transporter.State == ResonanceComponentState.Connected)
            {
                var audioMessage = new AudioMessage(senderId: client.ClientID, e.Buffer);
                await Transporter.SendAsync(audioMessage);
            }
            else
            {
                if(IsEnableLog)
                    Point.L.M("Cannot send audio message. Transporter is not connected.");
            }
        };

        Transporter.StateChanged += async (s, e) =>
        {
            if (Transporter.State == ResonanceComponentState.Disconnected)
            {
                if (IsEnableLog)
                    Point.L.M("Transporter disconnected. Attempting to reconnect...");
                try
                {
                    await Transporter.ConnectAsync();
                    if (IsEnableLog)
                        Point.L.M("Reconnected successfully.");
                }
                catch (Exception ex)
                {
                    if (IsEnableLog)
                        Point.L.M($"Reconnection failed: {ex.Message}");
                }
            }
        };

        _waveIn.StartRecording();
        if (IsEnableLog)
            Point.L.M("Audio capture started.");
    }

    public void SetupAudioPlayback()
    {
        _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1));
        _waveOut = new WaveOutEvent();
        _waveOut.Init(_bufferedWaveProvider);
        _waveOut.Play();
    }
}

public class Client : IPoint
{
    public Int16? ClientID;
    private IResonanceTransporter? _transporter;
    private List<ClientElement>? _connectedClients = [];
    public Action<object> ActionMessageSender;

    public Client()
    {
        Settings = new PointSettings();
        Type = PointType.Client;
        L = new L(this);

        Audio = new AudioClient(this);
    }

    private RSAEncryption RSAEncryption { get; set; } = new RSAEncryption();
    public AudioClient Audio { get; set; }

    public Action<TextMessage>? OnTextMessageReceive { get; set; }
    public PointType Type { get; set; }
    public L L { get; set; }
    public PointSettings Settings { get; set; }

    public async Task Connect(string remoteIp, int port)
    {
        L.M("Trying to connect...");
        _transporter = await BuildConnect(remoteIp, port);
    }

    private async Task<IResonanceTransporter?> BuildConnect(string remoteIp, int port)
    {
        if (_transporter == null)
        {
            IResonanceTransporter transporter = new ResonanceTransporter();
            transporter.Adapter = new TcpAdapter(remoteIp, port);

            transporter.StateChanged += async (s, e) =>
            {
                if (_transporter == null)
                    return;

                if (transporter.State == ResonanceComponentState.Connected)
                {
                    L.M($"Client: {typeof(Client).Name} Connected!");
                }
            };

            transporter.MessageReceived += async (s, e) => await MessageHandler(s, e);

            try
            {
                await transporter.ConnectAsync();
                await transporter.SendAsync(new KeyMessage(RSAEncryption.GetPublicKey()));
                L.M("Connection successful.");
                Console.Clear();

                if (Settings.OnStartAudioPlayback)
                    Audio.SetupAudioPlayback();
            }
            catch (Exception ex)
            {
                L.E($"Connection failed: {ex.Message}");
            }

            return transporter;
        }

        return null;
    }

    private async Task MessageHandler(object s, ResonanceMessageReceivedEventArgs e)
    {
        if (e.Message.Object is SyncMessage sync)
        {
            _connectedClients = sync.ConnectedClients;
        }

        if (e.Message.Object is ConnectMessage clientsMessage)
        {
            ClientID = clientsMessage.ClientID;

            _connectedClients = clientsMessage.ConnectedClients;

            L.M($"My client ID: {ClientID}");
        }

        if (e.Message.Object is TextMessage textMessage)
        {
            if (textMessage.Content == null)
                return;

            string content = RSAEncryption.Decrypt(textMessage.Content);

            if (content != string.Empty)
            {
                textMessage.EncryptedText = content;
                OnTextMessageReceive?.Invoke(textMessage);
                Console.WriteLine(content);
            }
        }

        ActionMessageSender?.Invoke(e.Message.Object);
    }

    public async Task SendTextMessage(string text)
    {
        foreach (var client in _connectedClients)
        {
            var param = client.Parameters;
            var encryptedContent = RSAEncryption.Encrypt(text, param);
            var message = new TextMessage(ClientID, encryptedContent);
            if (_transporter != null)
                await _transporter.SendAsync(message);
        }

        L.M($"message [{text}] has been sent.");
    }

    public IResonanceTransporter GetTransporter()
    {
        if(_transporter != null)
            return _transporter;
        else return new ResonanceTransporter();
    }

    public RSAParameters GetPublicKey()
    {
        return RSAEncryption.GetPublicKey();
    }
}

