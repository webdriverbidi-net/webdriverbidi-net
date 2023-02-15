// <copyright file="WebSocketServer.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A simple in-memory web server that can serve have its protocol upgraded from
/// HTTP to using a WebSocket. This server uses a TcpListener instead of an
/// HttpListener so as to avoid the restriction of having to register non-localhost
/// prefixes as an admin on Windows.
/// </summary>
public class WebSocketServer : WebServer
{
    private static readonly string WebSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    private static readonly byte ParityBit = 0x80;
    private WebSocketState state = WebSocketState.None;
    private bool ignoreCloseRequest = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketServer"/> class listening on a random port.
    /// </summary>
    public WebSocketServer()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketServer"/> class listening on a specific port.
    /// </summary>
    /// <param name="port">The port on which to listen. Passing zero (0) for the port will select a random port.</param>
    public WebSocketServer(int port)
        : base(port)
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether the server should ignore requests from the client to close the WebSocket.
    /// </summary>
    public bool IgnoreCloseRequest { get => this.ignoreCloseRequest; set => this.ignoreCloseRequest = value; }

    /// <summary>
    /// Gets a value indicating whether the server should continue listening for incoming connections.
    /// </summary>
    protected override bool ContinueRunning
    {
        get { return base.ContinueRunning && this.state != WebSocketState.Closed; }
    }

    /// <summary>
    /// Asynchronously sends data to the connected WebSocket client.
    /// </summary>
    /// <param name="data">A string representing the data to be sent.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task SendData(string data)
    {
        WebSocketFrameData frame = this.EncodeData(data);
        await this.SendData(frame.Data);
    }

    /// <summary>
    /// Asynchrounously forcibly disconnects the server without following the appropriate shutdown procedure.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override async Task Disconnect()
    {
        if (this.HasClientSocket && this.state == WebSocketState.Open)
        {
            await this.SendCloseFrame("Initiating close");
            this.state = WebSocketState.CloseSent;
        }
    }

    /// <summary>
    /// Asynchronously handles an HTTP request, including requests to upgrade the protocol to use a WebSocket.
    /// </summary>
    /// <param name="requestBuffer">A byte array buffer containing the data of the HTTP request.</param>
    /// <param name="requestLength">The length of the data in the HTTP request.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async Task HandleHttpRequest(byte[] requestBuffer, int requestLength)
    {
        string rawRequest = Encoding.UTF8.GetString(requestBuffer, 0, requestLength);
        HttpRequest request = HttpRequest.Parse(rawRequest);
        if (request.Headers.ContainsKey("Connection") && request.Headers["Connection"].Contains("Upgrade") &&
            request.Headers.ContainsKey("Upgrade") && request.Headers["Upgrade"].Contains("websocket") &&
            request.Headers.ContainsKey("Sec-WebSocket-Key"))
        {
            this.LogMessage($"=====Handshaking from client=====\n{rawRequest}");
            this.state = WebSocketState.Connecting;
            await this.PerformHandshake(request.Headers["Sec-WebSocket-Key"][0]);
            this.state = WebSocketState.Open;
            return;
        }

        await base.HandleHttpRequest(requestBuffer, requestLength);
    }

    /// <summary>
    /// Asynchronously processes incoming data from the client.
    /// </summary>
    /// <param name="buffer">A byte array buffer containing the data.</param>
    /// <param name="receivedLength">The length of the data in the buffer.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async Task ProcessIncomingData(byte[] buffer, int receivedLength)
    {
        this.LogMessage($"RECV {receivedLength} bytes");
        if (this.state == WebSocketState.None)
        {
            await this.HandleHttpRequest(buffer, receivedLength);
        }
        else
        {
            var frame = this.DecodeData(buffer);
            if (frame.Opcode == WebSocketOpcodeType.Text)
            {
                string text = Encoding.UTF8.GetString(frame.Data);
                this.OnDataReceived(new WebServerDataReceivedEventArgs(text));
            }

            if (frame.Opcode == WebSocketOpcodeType.ClosedConnection)
            {
                if (this.state == WebSocketState.Open && !this.ignoreCloseRequest)
                {
                    this.state = WebSocketState.CloseReceived;
                    await this.SendCloseFrame("Acknowledge close");
                }

                this.state = WebSocketState.Closed;
            }
        }
    }

    private async Task PerformHandshake(string websocketSecureKey)
    {
        // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
        // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
        // 3. Compute SHA-1 and Base64 hash of the new value
        // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
        string websocketSecureResponse = websocketSecureKey + WebSocketGuid;
        byte[] websocketResponseHash = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(websocketSecureResponse));
        string websocketHashBase64 = Convert.ToBase64String(websocketResponseHash);

        HttpResponse responseData = new()
        {
            StatusCode = HttpStatusCode.SwitchingProtocols,
        };
        responseData.Headers["Connection"] = new List<string>() { "Upgrade" };
        responseData.Headers["Upgrade"] = new List<string>() { "websocket" };
        responseData.Headers["Sec-WebSocket-Accept"] = new List<string>() { websocketHashBase64 };
        byte[] response = responseData.ToByteArray();
        await this.SendData(response);
    }

    private async Task SendCloseFrame(string message)
    {
        WebSocketFrameData closeFrame = this.EncodeData(message, WebSocketOpcodeType.ClosedConnection);
        await this.SendData(closeFrame.Data);
    }

    private WebSocketFrameData DecodeData(byte[] buffer)
    {
        int keyOffset = 0;
        const byte opcodeMask = 0x0F;
        const byte messageLengthMask = 0x7F;
        WebSocketOpcodeType opcode = (WebSocketOpcodeType)(buffer[0] & opcodeMask);
        this.LogMessage($"Decoding data with opcode {opcode}");

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
        ArraySegment<byte> key = new(buffer, keyOffset, 4);
        ulong offset = Convert.ToUInt64(keyOffset + key.Count);
        for (ulong index = 0; index < messageLength; index++)
        {
            decoded[index] = (byte)(buffer[offset + index] ^ key[Convert.ToInt32(index % 4)]);
        }

        return new WebSocketFrameData(opcode, decoded);
    }

    private WebSocketFrameData EncodeData(string data, WebSocketOpcodeType opcode = WebSocketOpcodeType.Text)
    {
        this.LogMessage($"Encoding data with opcode {opcode}");
        if (opcode == WebSocketOpcodeType.ClosedConnection)
        {
            // NOTE: Hard code the close frame data.
            return new WebSocketFrameData(opcode, new byte[] { 0x88, 0x00 });
        }

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