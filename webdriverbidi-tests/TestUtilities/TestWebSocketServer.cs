namespace WebDriverBidi.TestUtilities;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Enum for opcode types
/// </summary>
public enum OpcodeType
{
    /* Denotes a continuation code */
    Fragment = 0,

    /* Denotes a text code */
    Text = 1,

    /* Denotes a binary code */
    Binary = 2,

    /* Denotes a closed connection */
    ClosedConnection = 8,

    /* Denotes a ping*/
    Ping = 9,

    /* Denotes a pong */
    Pong = 10
}

public class WebSocketFrameData
{
    private OpcodeType opcode;
    private byte[] data;
    public WebSocketFrameData(OpcodeType opcode, byte[] data)
    {
        this.opcode = opcode;
        this.data = data;
    }

    public OpcodeType Opcode => this.opcode;
    public byte[] Data => this.data;
}

public class TestWebSocketServer
{
    // This is a special GUID specified by the Web Socket spec (RFC 6455)
    private static readonly string WebSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    private static readonly byte ParityBit = 0x80;

    private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private Socket? clientSocket;
    private int port = 0;
    private CancellationTokenSource listenerCancelationTokenSource = new CancellationTokenSource();
    private List<string> serverLog = new List<string>();

    private Task? foo;

    public TestWebSocketServer()
    {
    }

    public int Port => this.port;

    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    public void Start()
    {
        IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Loopback, this.port);
        this.socket.Bind(serverEndpoint);
        IPEndPoint? localEndpoint = this.socket.LocalEndPoint as IPEndPoint;
        if (localEndpoint is not null)
        {
            this.port = localEndpoint.Port;
        }
        
        foo = Task.Run(() => this.ReceiveData().ConfigureAwait(false));
    }

    public void Stop()
    {
        this.listenerCancelationTokenSource.Cancel();
        this.CloseSocket();
    }

    public async Task SendData(string data)
    {
        if (this.clientSocket is null)
        {
            throw new Exception("No attached client");
        }

        WebSocketFrameData frame = EncodeData(data);
        int bytesSent = await this.clientSocket.SendAsync(frame.Data, SocketFlags.None);
        this.serverLog.Append($"SEND {bytesSent} bytes");
    }

    protected virtual void OnDataReceived(DataReceivedEventArgs e)
    {
        if (this.DataReceived is not null)
        {
            this.DataReceived(this, e);
        }
    }

    private async Task ReceiveData()
    {
        this.socket.Listen();
        this.clientSocket = await this.socket.AcceptAsync(this.listenerCancelationTokenSource.Token);
        this.serverLog.Append("Socket connected");
        while (!this.listenerCancelationTokenSource.Token.IsCancellationRequested)
        {
            var buffer = new byte[1024];
            var receivedLength = await clientSocket.ReceiveAsync(buffer, SocketFlags.None, this.listenerCancelationTokenSource.Token);
            this.serverLog.Append($"RECV {receivedLength} bytes");
            var data = Encoding.UTF8.GetString(buffer, 0, receivedLength);
            if (Regex.IsMatch(data, "^GET", RegexOptions.IgnoreCase)) {
                await this.PerformHandshake(data);
            } 
            else 
            {
                var frame = this.DecodeData(buffer);
                if (frame.Opcode == OpcodeType.Text)
                {
                    string text = Encoding.UTF8.GetString(frame.Data);
                    this.OnDataReceived(new DataReceivedEventArgs(text));
                }

                if (frame.Opcode == OpcodeType.ClosedConnection)
                {
                    this.listenerCancelationTokenSource.Cancel();
                }
            }
        }

        await this.CloseClientSocket();
    }

    private void CloseSocket()
    {
        if (this.socket.Connected)
        {
            this.socket.Shutdown(SocketShutdown.Both);
            this.serverLog.Append("Socket disconnected");
        }
        this.socket.Close();
        this.socket.Dispose();
    }

    private async Task CloseClientSocket()
    {
        if (this.clientSocket is not null)
        {
            try
            {
               this.serverLog.Append("Closing client socket");
                WebSocketFrameData closeFrame = EncodeData("Acknowledge close", OpcodeType.ClosedConnection);
                await this.clientSocket.SendAsync(closeFrame.Data, SocketFlags.None);
                this.clientSocket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                this.clientSocket.Close();
                this.clientSocket.Dispose();
                this.clientSocket = null;
            }
        }
    }

    private async Task PerformHandshake(string data)
    {
        if (this.clientSocket is null)
        {
            throw new Exception("No client connection");
        }
        this.serverLog.Append($"=====Handshaking from client=====\n{data}");

        // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
        // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
        // 3. Compute SHA-1 and Base64 hash of the new value
        // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
        string websocketSecureKey = Regex.Match(data, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
        string websocketSecureResponse = websocketSecureKey + WebSocketGuid;
        byte[] websocketResponseHash = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(websocketSecureResponse));
        string websocketHashBase64 = Convert.ToBase64String(websocketResponseHash);

        // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
        byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols\r\n" +
            "Connection: Upgrade\r\n" +
            "Upgrade: websocket\r\n" +
            "Sec-WebSocket-Accept: " + websocketHashBase64 + "\r\n\r\n");
        await this.clientSocket.SendAsync(response, SocketFlags.None);
    }

    private WebSocketFrameData DecodeData(byte[] buffer)
    {
        int keyOffset = 0;
        const byte opcodeMask = 0x0F;
        const byte messageLengthMask = 0x7F;
        bool fin = (buffer[0] & ParityBit) != 0;
        bool mask = (buffer[1] & ParityBit) != 0;
        OpcodeType opcode = (OpcodeType)(buffer[0] & opcodeMask);
        this.serverLog.Append($"Decoding data with opcode {opcode}");

        ulong messageLength = Convert.ToUInt64(buffer[1] & messageLengthMask);

        if (messageLength <= 125)
        {
            keyOffset = 2;
        }

        if (messageLength == 126)
        {
            messageLength = BitConverter.ToUInt64(new byte[] { buffer[3], buffer[2] }, 0);
            keyOffset = 4;
        }

        if (messageLength == 127)
        {
            messageLength = BitConverter.ToUInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
            keyOffset = 10;
        }

        byte[] decoded = new byte[messageLength];
        ArraySegment<byte> key = new ArraySegment<byte>(buffer, keyOffset, 4);
        ulong offset = Convert.ToUInt64(keyOffset + key.Count);
        for (ulong index = 0; index < messageLength; index++)
        {
            decoded[index] = (byte)(buffer[offset + index] ^ key[Convert.ToInt32(index % 4)]);
        }

        return new WebSocketFrameData(opcode, decoded);
    }

    public WebSocketFrameData EncodeData(string data, OpcodeType opcode = OpcodeType.Text)
    {
       this.serverLog.Append($"Encoding data with opcode {opcode}");
        long dataOffset = -1;
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        byte[] frame = new byte[10];
        frame[0] = Convert.ToByte(Convert.ToByte(opcode) | ParityBit);
        long length = dataBytes.LongLength;

        if (length <= 125)
        {
            frame[1] = Convert.ToByte(length);
            dataOffset = 2;
        }

        if (length >= 126 && length <= 65535)
        {
            frame[1] = 126;
            frame[2] = Convert.ToByte((length >> 8) & 255);
            frame[3] = Convert.ToByte(length & 255);
            dataOffset = 4;
        }

        if (length >= 65536)
        {
            frame[1] = 127;
            frame[2] = Convert.ToByte((length >> 56) & 255);
            frame[3] = Convert.ToByte((length >> 48) & 255);
            frame[4] = Convert.ToByte((length >> 40) & 255);
            frame[5] = Convert.ToByte((length >> 32) & 255);
            frame[6] = Convert.ToByte((length >> 24) & 255);
            frame[7] = Convert.ToByte((length >> 16) & 255);
            frame[8] = Convert.ToByte((length >> 8) & 255);
            frame[9] = Convert.ToByte(length & 255);
            dataOffset = 10;
        }

        byte[] buffer = new byte[dataOffset + length];
        frame.CopyTo(buffer, 0);
        dataBytes.CopyTo(buffer, dataOffset);
        return new WebSocketFrameData(opcode, buffer);
    }
}