namespace WebDriverBidi;

using System.Net.WebSockets;
using System.Text;

public class Connection
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private bool isActive = false;
    private readonly int bufferSize = 4096;
    private ClientWebSocket client = new();
    private readonly CancellationTokenSource clientTokenSource = new();
    private readonly TimeSpan startupTimeout;
    private readonly TimeSpan shutdownTimeout;

    public Connection() : this(DefaultTimeout)
    {
    }

    public Connection(TimeSpan startupTimeout) : this(startupTimeout, DefaultTimeout)
    {
    }

    public Connection(TimeSpan startupTimeout, TimeSpan shutdownTimeout)
    {
        this.startupTimeout = startupTimeout;
        this.shutdownTimeout = shutdownTimeout;
    }

    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler<LogMessageEventArgs>? LogMessage;

    public bool IsActive => this.isActive;

    public int BufferSize => this.bufferSize;

    public virtual async Task Start(string url)
    {
        this.Log($"Opening connection to URL {url}", WebDriverBidiLogLevel.Info);
        bool connected = false;
        DateTime timeout = DateTime.Now.Add(this.startupTimeout);
        while (!connected && DateTime.Now <= timeout)
        {
            try
            {
                await this.client.ConnectAsync(new Uri(url), clientTokenSource.Token);
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

    public virtual async Task SendData(string data)
    {
        ArraySegment<byte> messageBuffer = new(Encoding.UTF8.GetBytes(data));
        this.Log($"SEND >>> {data}");
        await this.client.SendAsync(messageBuffer, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
    }

    protected virtual void OnDataReceived(DataReceivedEventArgs e)
    {
        if (this.DataReceived is not null)
        {
            this.DataReceived(this, e);
        }
    }

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
                var receiveResult = await client.ReceiveAsync(buffer, cancellationToken);
                // if the token is cancelled while ReceiveAsync is blocking, the socket state changes to aborted and it can't be used
                if (!cancellationToken.IsCancellationRequested)
                {
                    // the server is notifying us that the connection will close; send acknowledgement
                    if (this.client.State == WebSocketState.CloseReceived && receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        this.Log($"Acknowledging Close frame received from server", WebDriverBidiLogLevel.Info);
                        await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None);
                    }

                    // display text or binary data
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
                                this.OnDataReceived(new DataReceivedEventArgs(message));
                            }
                        }
                    }
                }
            }

            this.Log($"Ending processing loop in state {client.State}", WebDriverBidiLogLevel.Info);
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
