// <copyright file="Connection.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Protocol;

using System.Net.WebSockets;
using System.Text;

/// <summary>
/// Represents a connection to a WebDriver Bidi remote end.
/// </summary>
public class Connection
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private readonly CancellationTokenSource clientTokenSource = new();
    private readonly TimeSpan startupTimeout;
    private readonly TimeSpan shutdownTimeout;
    private readonly int bufferSize = 4096;
    private bool isActive = false;
    private ClientWebSocket client = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection" /> class.
    /// </summary>
    public Connection()
        : this(DefaultTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection" /> class with a given startup timeout.
    /// </summary>
    /// <param name="startupTimeout">The timeout before throwing an error when starting up the connection.</param>
    public Connection(TimeSpan startupTimeout)
        : this(startupTimeout, DefaultTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection" /> class with a given startup and shutdown timeout.
    /// </summary>
    /// <param name="startupTimeout">The timeout before throwing an error when starting up the connection.</param>
    /// <param name="shutdownTimeout">The timeout before throwing an error when shutting down the connection.</param>
    public Connection(TimeSpan startupTimeout, TimeSpan shutdownTimeout)
    {
        this.startupTimeout = startupTimeout;
        this.shutdownTimeout = shutdownTimeout;
    }

    /// <summary>
    /// Occurs when data is received from this connection.
    /// </summary>
    public event EventHandler<ConnectionDataReceivedEventArgs>? DataReceived;

    /// <summary>
    /// Occurs when a log message is emitted from this connection.
    /// </summary>
    public event EventHandler<LogMessageEventArgs>? LogMessage;

    /// <summary>
    /// Gets a value indicating whether this connection is active.
    /// </summary>
    public bool IsActive => this.isActive;

    /// <summary>
    /// Gets the buffer size for communication used by this connection.
    /// </summary>
    public int BufferSize => this.bufferSize;

    /// <summary>
    /// Asynchronously starts communication with the remote end of this connection.
    /// </summary>
    /// <param name="url">The URL used to connect to the remote end.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="TimeoutException">Thrown when the connection is not established within the startup timeout.</exception>
    public virtual async Task Start(string url)
    {
        this.Log($"Opening connection to URL {url}", WebDriverBidiLogLevel.Info);
        bool connected = false;
        DateTime timeout = DateTime.Now.Add(this.startupTimeout);
        while (!connected && DateTime.Now <= timeout)
        {
            try
            {
                await this.client.ConnectAsync(new Uri(url), this.clientTokenSource.Token);
                connected = true;
            }
            catch (WebSocketException)
            {
                // If the server-side socket is not yet ready, it leaves the client socket in a closed state,
                // which sees the object as disposed, so we must create a new one to try again
                await Task.Delay(500);
                this.client = new ClientWebSocket();
            }
        }

        if (!connected)
        {
            throw new TimeoutException($"Could not connect to browser within {this.startupTimeout.TotalSeconds} seconds");
        }

        _ = Task.Run(() => this.ReceiveData().ConfigureAwait(false));
        this.isActive = true;
        this.Log($"Connection opened", WebDriverBidiLogLevel.Info);
    }

    /// <summary>
    /// Asynchronously stops communication with the remote end of this connection.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task Stop()
    {
        this.Log($"Closing connection", WebDriverBidiLogLevel.Info);
        if (this.client.State != WebSocketState.Open)
        {
            this.Log($"Socket already closed. ({this.client.State}");
            return;
        }

        // Close the socket first, because ReceiveAsync leaves an invalid socket (state = aborted) when the token is cancelled
        CancellationTokenSource timeout = new(this.shutdownTimeout);
        try
        {
            // After this, the socket state which change to CloseSent
            await this.client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token);

            // Now we wait for the server response, which will close the socket
            while (this.client.State != WebSocketState.Closed && !timeout.Token.IsCancellationRequested)
            {
                // The loop may be too tight for the cancellation token to get triggered, so add a small delay
                await Task.Delay(10);
            }

            this.Log($"Client state is {this.client.State}", WebDriverBidiLogLevel.Info);
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledExcetption is normal upon task/token cancellation, so disregard it
        }

        // Whether we closed the socket or timed out, we cancel the token causing RecieveAsync to abort the socket.
        // The finally block at the end of the processing loop will dispose of the ClientWebSocket object.
        this.clientTokenSource.Cancel();
    }

    /// <summary>
    /// Asynchronously sends data to the remote end of this connection.
    /// </summary>
    /// <param name="data">The data to be sent to the remote end of this connection.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task SendData(string data)
    {
        ArraySegment<byte> messageBuffer = new(Encoding.UTF8.GetBytes(data));
        this.Log($"SEND >>> {data}");
        await this.client.SendAsync(messageBuffer, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
    }

    /// <summary>
    /// Raises the DataReceived event.
    /// </summary>
    /// <param name="e">The event args used when raising the event.</param>
    protected virtual void OnDataReceived(ConnectionDataReceivedEventArgs e)
    {
        if (this.DataReceived is not null)
        {
            this.DataReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the LogMessage event.
    /// </summary>
    /// <param name="e">The event args used when raising the event.</param>
    protected virtual void OnLogMessage(LogMessageEventArgs e)
    {
        if (this.LogMessage is not null)
        {
            this.LogMessage(this, e);
        }
    }

    private async Task ReceiveData()
    {
        var cancellationToken = this.clientTokenSource.Token;
        try
        {
            StringBuilder messageBuilder = new();
            var buffer = WebSocket.CreateClientBuffer(this.bufferSize, this.bufferSize);
            while (this.client.State != WebSocketState.Closed && !cancellationToken.IsCancellationRequested)
            {
                var receiveResult = await this.client.ReceiveAsync(buffer, cancellationToken);

                // If the token is cancelled while ReceiveAsync is blocking, the socket state changes to aborted and it can't be used
                if (!cancellationToken.IsCancellationRequested)
                {
                    // The server is notifying us that the connection will close; send acknowledgement
                    if (this.client.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        this.Log($"Acknowledging Close frame received from server", WebDriverBidiLogLevel.Info);
                        await this.client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None);
                    }

                    // Display text or binary data
                    if (this.client.State == WebSocketState.Open && receiveResult.MessageType != WebSocketMessageType.Close)
                    {
                        messageBuilder.Append(Encoding.UTF8.GetString(buffer.Array ?? Array.Empty<byte>(), 0, receiveResult.Count));
                        if (receiveResult.EndOfMessage)
                        {
                            string message = messageBuilder.ToString();
                            messageBuilder = new StringBuilder();
                            if (message.Length > 0)
                            {
                                this.Log($"RECV <<< {message}");
                                this.OnDataReceived(new ConnectionDataReceivedEventArgs(message));
                            }
                        }
                    }
                }
            }

            this.Log($"Ending processing loop in state {this.client.State}", WebDriverBidiLogLevel.Info);
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledExcetption is normal upon task/token cancellation, so disregard it
        }
        catch (Exception e)
        {
            throw new WebDriverBidiException($"Unexpected error during receive of data: {e.Message}", e);
        }
        finally
        {
            this.client.Dispose();
            this.isActive = false;
        }
    }

    private void Log(string message)
    {
        this.Log(message, WebDriverBidiLogLevel.Debug);
    }

    private void Log(string message, WebDriverBidiLogLevel level)
    {
        this.OnLogMessage(new LogMessageEventArgs(message, level));
    }
}
