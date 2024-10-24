using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Connector;

public enum ReceiveType
{
    Bytes,
    UTF8Text,
    Audio,
    Video
}

public enum ConnectionType
{
    Server,
    Client
}

public class CConnector
{
    public CConnector()
    {

    }

    public Udp UdpClient { get; set; }
    public Tcp TcpClient { get; set; }

    public class Udp: IDisposable
    {
        public Udp(IPAddress address, Int32 port)
        {
            Client = new UdpClient();

            Client.Connect(address, port);
        }

        private UdpClient Client { get; set; }

        public async Task Send(byte[] data)
        {
            int bytes = await Client.SendAsync(data);
        }

        public async Task Send(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            int bytes = await Client.SendAsync(data);
        }

        public async Task<UdpReceiveResult> Receive(ReceiveType type, bool isDebug = true)
        {
            var result = await Client.ReceiveAsync();

            if(type == ReceiveType.UTF8Text)
            {
                var message = Encoding.UTF8.GetString(result.Buffer);
                Console.WriteLine(message);
            }

            if (isDebug)
            {
                Console.WriteLine($"Receive {result.Buffer.Length} bytes");
                Console.WriteLine($"Remote IP: {result.RemoteEndPoint}");
            }

            return result;
        }

        public void Dispose()
        {

        }
    }

    public class Tcp
    {
        private static CancellationToken Token { get; set; }
        private SocketAsyncEventArgs _sendSocketEvent;
        private SocketAsyncEventArgs _recvSocketEvent;

        internal static BufferPool RecvBufferPool = new BufferPool("Receive", 2048, 2048);

        private byte[] _recvBuffer;

        internal static SocketAsyncEventArgsPool SendSocketEventPool = new SocketAsyncEventArgsPool(10);
        internal static SocketAsyncEventArgsPool RecvSocketEventPool = new SocketAsyncEventArgsPool(10);

        /// <summary>
        /// it is recommended to use IPAddress.Any for TCP server
        /// </summary>
        public Tcp(ConnectionType connectionType, IPAddress address, Int32 port, bool isDebug = true)
        {
            TcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Server field
            if (connectionType == ConnectionType.Server)
            {
                IPEndPoint ipPoint = new IPEndPoint(address, port);

                try
                {
                    TcpListener.Bind(ipPoint);
                    TcpListener.Listen();

                    if (isDebug)
                        Console.WriteLine($"TCP Server ({TcpListener.Available}) is running. Waiting for connections... ");

                    Console.Title = "TCP SERVER";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else if (connectionType == ConnectionType.Client)
            {
                TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (isDebug)
                    Console.WriteLine("TCP Client is running. Wait for server connection.");

                Console.Title = "TCP CLIENT";
            }

            _sendSocketEvent = SendSocketEventPool.Pop();

            Token = new CancellationToken();
        }

        /// <summary>
        /// Server side TCP
        /// </summary>
        public Socket TcpListener { get; set; }

        /// <summary>
        /// Client side TCP
        /// </summary>
        public Socket TcpClient { get; set; }

        public async Task ServerSend(byte[] data, bool isDebug = true)
        {
            try
            {
                using var tcpClient = await TcpListener.AcceptAsync();

                await tcpClient.SendAsync(data);

                if (isDebug)
                    Console.WriteLine($"Data has been sent to the {tcpClient.RemoteEndPoint} client");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// bufferSize default value is 500
        /// </summary>
        public async Task<byte[]> ServerReceive(ReceiveType receiveType, int bufferSize, bool isDebug = true)
        {
            List<byte> response = [];
            byte[] buffer = new byte[bufferSize];
            int bytes = 0;

            var socket = await TcpListener.AcceptAsync();
            bytes = await socket.ReceiveAsync(buffer);

            response.AddRange(buffer.Take(bytes));

            if (isDebug)
            {
                Console.WriteLine($"Bytes length {bytes}");

                Console.WriteLine("Received bytes:\n");

                response.ForEach(x => Console.WriteLine());
            }

            return response.ToArray();
        }

        public async Task ConnectToServer(IPAddress address, Int32 port, bool isDebug = true)
        {
            await TcpClient.ConnectAsync(address, port);

            if(isDebug)
                Console.WriteLine($"Connected to {TcpClient.RemoteEndPoint}");
        }

        public void SendToServer(byte[] data, bool isDebug = true)
        {
            _sendSocketEvent.SetBuffer(data, 0, data.Length);
            TcpClient.SendAsync(_sendSocketEvent);

            if(isDebug)
                Console.WriteLine($"Data: {data} sended to {TcpClient.RemoteEndPoint}.");
        }

        public async Task<int> ReceiveFromServer(ReceiveType receiveType, bool isDebug = true)
        {
            try
            {
                // buffer
                byte[] data = new byte[512];

                int bytes = await TcpClient.ReceiveAsync(data);

                if (isDebug)
                    Console.WriteLine($"Received: {bytes} from {TcpClient.RemoteEndPoint}");

                //if(receiveType == ReceiveType.UTF8Text)
                //{
                //    var message = Encoding.UTF8.GetString();
                //    Console.WriteLine(message);
                //}

                return bytes;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }
    }

    private IPAddress? LocalIPAddress()
    {
        if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
        {
            return null;
        }

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        return host
            .AddressList
            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public static string GetLocalIPAddresFromSocket()
    {
        string localIP;
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint.Address.ToString();
        }

        return localIP;
    }

    public static string GetLocalIPv4(NetworkInterfaceType _type)
    {
        string output = "";
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
            {
                IPInterfaceProperties adapterProperties = item.GetIPProperties();
                if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                {
                    foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                            break;
                        }
                    }
                }
            }
            if (output != "") { break; }
        }
        return output;
    }
}

public class BufferPool
{
    private static List<BufferPool> m_Pools = new List<BufferPool>();

    public static List<BufferPool> Pools { get { return m_Pools; } set { m_Pools = value; } }

    private string m_Name;

    private int m_InitialCapacity;
    private int m_BufferSize;

    private int m_Misses;

    private Queue<byte[]> m_FreeBuffers;

    public void GetInfo(out string name, out int freeCount, out int initialCapacity, out int currentCapacity, out int bufferSize, out int misses)
    {
        lock (this)
        {
            name = m_Name;
            freeCount = m_FreeBuffers.Count;
            initialCapacity = m_InitialCapacity;
            currentCapacity = m_InitialCapacity * (1 + m_Misses);
            bufferSize = m_BufferSize;
            misses = m_Misses;
        }
    }

    public BufferPool(string name, int initialCapacity, int bufferSize)
    {
        m_Name = name;

        m_InitialCapacity = initialCapacity;
        m_BufferSize = bufferSize;

        m_FreeBuffers = new Queue<byte[]>(initialCapacity);

        for (int i = 0; i < initialCapacity; ++i)
            m_FreeBuffers.Enqueue(new byte[bufferSize]);

        lock (m_Pools)
            m_Pools.Add(this);
    }

    public byte[] AcquireBuffer()
    {
        lock (this)
        {
            if (m_FreeBuffers.Count > 0)
                return m_FreeBuffers.Dequeue();

            ++m_Misses;

            for (int i = 0; i < m_InitialCapacity; ++i)
                m_FreeBuffers.Enqueue(new byte[m_BufferSize]);

            return m_FreeBuffers.Dequeue();
        }
    }

    public void ReleaseBuffer(byte[] buffer)
    {
        if (buffer == null)
            return;

        lock (this)
            m_FreeBuffers.Enqueue(buffer);
    }

    public void Free()
    {
        lock (m_Pools)
            m_Pools.Remove(this);
    }
}

public class SocketAsyncEventArgsPool
{
    private ConcurrentStack<SocketAsyncEventArgs> m_EventsPool;

    public SocketAsyncEventArgsPool(int numConnection)
    {
        m_EventsPool = new ConcurrentStack<SocketAsyncEventArgs>();
    }

    public SocketAsyncEventArgs Pop()
    {
        if (m_EventsPool.IsEmpty)
            return new SocketAsyncEventArgs();

        SocketAsyncEventArgs popped;
        m_EventsPool.TryPop(out popped);

        return popped;
    }

    public void Push(SocketAsyncEventArgs item)
    {
        if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }

        m_EventsPool.Push(item);
    }

    public int Count
    {
        get { return m_EventsPool.Count; }
    }

    public void Dispose()
    {
        foreach (SocketAsyncEventArgs e in m_EventsPool)
        {
            e.Dispose();
        }

        m_EventsPool.Clear();
    }
}

public static class StringExtention
{
    public static byte[] StringToByteArray(this String _string)
    {
        byte[] data = Encoding.UTF8.GetBytes(_string);

        return data;
    }

    public static string ByteArrayToUTF8String(this byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    }
}
