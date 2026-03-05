// <copyright file="PipeConnection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

/// <summary>
/// Represents a connection to a WebDriver Bidi remote end over anonymous pipes.
/// This is used with Chromium's --remote-debugging-pipe flag, which on non-Windows
/// systems communicates via file descriptors 3 (browser reads) and 4 (browser writes).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PipeConnection"/> provides a specialized transport mechanism for browser communication
/// using anonymous pipes instead of WebSockets. This offers slightly lower latency but requires
/// the browser and application to be on the same machine.
/// </para>
/// <para>
/// <strong>When to consider pipe connections:</strong>
/// <list type="bullet">
/// <item><description>High-performance local test suites where latency is critical</description></item>
/// <item><description>Browser implementation supports --remote-debugging-pipe (currently only Chromium-based browsers)</description></item>
/// <item><description>Browser and tests run on the same machine</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Protocol details:</strong>
/// <list type="bullet">
/// <item><description>Messages are null-terminated JSON strings (each message ends with \0)</description></item>
/// <item><description>On Unix systems: Browser reads from file descriptor 3, writes to file descriptor 4</description></item>
/// <item><description>On Windows: Uses named pipe handles</description></item>
/// <item><description>Requires <see cref="IPipeServerProcessProvider"/> for process lifecycle management</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Limitations:</strong>
/// <list type="bullet">
/// <item><description>Only supported by Chromium-based browsers (Chrome, Edge)</description></item>
/// <item><description>Cannot connect to remote browsers</description></item>
/// <item><description>More complex setup than WebSocket connections</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Recommendation:</strong> Most users should use <see cref="WebSocketConnection"/> instead.
/// Pipe connections are only beneficial for specialized high-performance scenarios with Chromium browsers.
/// </para>
/// </remarks>
public class PipeConnection : Connection
{
    private readonly SemaphoreSlim dataSendSemaphore = new(1, 1);
    private readonly AnonymousPipeServerStream pipeToProcess;
    private readonly AnonymousPipeServerStream pipeFromProcess;
    private readonly IPipeServerProcessProvider processProvider;
    private Task? dataReceiveTask;
    private CancellationTokenSource connectionTokenSource = new();
    private int isConnectionActiveTypeSafeFlag = 0;
    private bool clientHandlesDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeConnection"/> class.
    /// </summary>
    /// <param name="processProvider">An implementation of <see cref="IPipeServerProcessProvider"/> that provides a <see cref="Process"/> that is able to send and receive messages over pipe connections.</param>
    public PipeConnection(IPipeServerProcessProvider processProvider)
    {
        // PipeDirection.Out means WE write to this pipe (browser will read from FD 3)
        this.pipeToProcess = new AnonymousPipeServerStream(
            PipeDirection.Out,
            HandleInheritability.Inheritable);

        // PipeDirection.In means WE read from this pipe (browser will write to FD 4)
        this.pipeFromProcess = new AnonymousPipeServerStream(
            PipeDirection.In,
            HandleInheritability.Inheritable);

        this.processProvider = processProvider;
    }

    /// <summary>
    /// Gets a value indicating whether this connection is active.
    /// </summary>
    public override bool IsActive => this.IsConnectionActive && this.processProvider.PipeServerProcess is not null && !this.processProvider.PipeServerProcess.HasExited;

    /// <summary>
    /// Gets a value indicating the type of data transport used by this connection, in this case, pipes.
    /// </summary>
    public override ConnectionType ConnectionType => ConnectionType.Pipes;

    /// <summary>
    /// Gets the handle used for sending data to the external process.
    /// </summary>
    public string ReadPipeHandle => this.clientHandlesDisposed ? string.Empty : this.pipeToProcess.GetClientHandleAsString();

    /// <summary>
    /// Gets the handle used for receiving data from the external process.
    /// </summary>
    public string WritePipeHandle => this.clientHandlesDisposed ? string.Empty : this.pipeFromProcess.GetClientHandleAsString();

    private bool IsConnectionActive
    {
        get
        {
            return Interlocked.CompareExchange(ref this.isConnectionActiveTypeSafeFlag, 0, 0) == 1;
        }

        set
        {
            int flagValue = value ? 1 : 0;
            Interlocked.Exchange(ref this.isConnectionActiveTypeSafeFlag, flagValue);
        }
    }

    /// <summary>
    /// Asynchronously starts communication with the remote end of this connection.
    /// </summary>
    /// <param name="connectionString">The connection string used to connect to the remote end.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the external application is not yet running, or the pipe connection is already connected.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public override async Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (this.processProvider.PipeServerProcess is null)
        {
            throw new WebDriverBiDiConnectionException("External process has not been set. Call SetExternalProcess before StartAsync.");
        }

        if (this.IsDisposed)
        {
            throw new WebDriverBiDiConnectionException("The pipes have been disposed; the connection cannot be restarted after disposal.");
        }

        if (this.IsConnectionActive)
        {
            throw new WebDriverBiDiConnectionException($"The pipe connection is already active for {this.ConnectionString}; call the Stop method to disconnect before calling Start");
        }

        await this.LogAsync($"Starting pipe connection: {connectionString}");

        // Dispose client handles in parent process only on first start - the external process has inherited them
        if (!this.clientHandlesDisposed)
        {
            this.pipeToProcess.DisposeLocalCopyOfClientHandle();
            this.pipeFromProcess.DisposeLocalCopyOfClientHandle();
            this.clientHandlesDisposed = true;
        }

        // Create a new cancellation token source for this connection session
        this.connectionTokenSource.Dispose();
        this.connectionTokenSource = new CancellationTokenSource();

        this.ConnectionString = connectionString;
        this.IsConnectionActive = true;

        // Start the receive loop
        this.dataReceiveTask = Task.Run(async () => await this.ReceiveDataAsync().ConfigureAwait(false));

        await this.LogAsync("Pipe connection started");
    }

    /// <summary>
    /// Asynchronously stops communication with the remote end of this connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.LogAsync("Closing pipe connection");

        if (!this.IsConnectionActive)
        {
            await this.LogAsync("Pipe connection already closed");
            return;
        }

        // Signal cancellation to stop the receive loop
        this.connectionTokenSource.Cancel();

        // Wait for the receive task to complete
        if (this.dataReceiveTask is not null)
        {
            await this.dataReceiveTask.ConfigureAwait(false);
        }

        this.IsConnectionActive = false;
        this.ConnectionString = string.Empty;

        await this.LogAsync("Pipe connection closed");
    }

    /// <summary>
    /// Asynchronously sends data to the remote end of this connection.
    /// </summary>
    /// <param name="data">The data to be sent to the remote end of this connection.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the pipe connection is not active.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when exclusive access to the pipe connection for sending times out.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public override async Task SendDataAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (!this.IsActive)
        {
            throw new WebDriverBiDiConnectionException("The pipe connection has not been initialized; you must call the Start method before sending data");
        }

        // Only one send operation at a time can be active on a pipe,
        // so we must synchronize send access to the pipe in case multiple threads are
        // attempting to send commands or other data simultaneously.
        if (!await this.dataSendSemaphore.WaitAsync(this.DataTimeout, cancellationToken).ConfigureAwait(false))
        {
            throw new WebDriverBiDiTimeoutException("Timed out waiting to access pipe for sending; only one send operation is permitted at a time.");
        }

        try
        {
            if (!this.IsActive)
            {
                throw new WebDriverBiDiConnectionException("The pipe connection was closed before the send could be completed");
            }

            if (this.OnLogMessage.CurrentObserverCount > 0)
            {
                await this.LogAsync($"SEND >>> {Encoding.UTF8.GetString(data)}", WebDriverBiDiLogLevel.Debug);
            }

            try
            {
                using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.connectionTokenSource.Token);
                await this.SendPipeDataAsync(data, linkedTokenSource.Token).ConfigureAwait(false);
            }
            catch (IOException ex)
            {
                throw new WebDriverBiDiConnectionException($"An error occurred while sending data: {ex.Message}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                throw new WebDriverBiDiConnectionException($"An error occurred while sending data: {ex.Message}", ex);
            }
        }
        finally
        {
            this.dataSendSemaphore.Release();
        }
    }

    /// <summary>
    /// Asynchronously sends data to the underlying pipe of this connection.
    /// </summary>
    /// <param name="messageBuffer">The buffer containing the data to be sent to the remote end of this connection via the pipe.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual async Task SendPipeDataAsync(byte[] messageBuffer, CancellationToken cancellationToken = default)
    {
        // Note use of the null-forgiving operator (!) here, as the pipeToProcess
        // should never be null when this method is called (it's only called after
        // IsActive check in SendDataAsync).
        // Write the data followed by a null terminator
        await this.pipeToProcess!.WriteAsync(messageBuffer, 0, messageBuffer.Length, cancellationToken).ConfigureAwait(false);
        byte[] nullTerminator = [0];
        await this.pipeToProcess.WriteAsync(nullTerminator, 0, 1, cancellationToken).ConfigureAwait(false);
        await this.pipeToProcess.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously reads data from the underlying pipe of this connection.
    /// </summary>
    /// <param name="buffer">The buffer to read data into.</param>
    /// <param name="offset">The offset in the buffer to start reading into.</param>
    /// <param name="count">The maximum number of bytes to read.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation, with a result containing the number of bytes read.</returns>
    protected virtual async Task<int> ReadPipeDataAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        // Note use of the null-forgiving operator (!) here, as the pipeFromProcess
        // should never be null when this method is called (it's only called after
        // StartAsync which ensures the pipes are initialized).
        return await this.pipeFromProcess!.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously receives data from the remote end of this connection.
    /// Messages are expected to be null-terminated as per the WebDriver BiDi pipe protocol.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async Task ReceiveDataAsync()
    {
        CancellationToken cancellationToken = this.connectionTokenSource.Token;
        MemoryStream messageBuffer = new();
        try
        {
            byte[] readBuffer = new byte[this.BufferSize];

            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await this.ReadPipeDataAsync(readBuffer, 0, readBuffer.Length, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    // Pipe closed
                    await this.LogAsync("Pipe closed by remote end");
                    break;
                }

                // Process the received data, looking for null terminators
                int startIndex = 0;
                for (int i = 0; i < bytesRead; i++)
                {
                    if (readBuffer[i] == 0)
                    {
                        // Found a null terminator - complete the message
                        if (i > startIndex)
                        {
                            messageBuffer.Write(readBuffer, startIndex, i - startIndex);
                        }

                        if (messageBuffer.Length > 0)
                        {
                            byte[] messageData = messageBuffer.ToArray();
                            messageBuffer.SetLength(0);

                            if (this.OnLogMessage.CurrentObserverCount > 0)
                            {
                                await this.LogAsync($"RECV <<< {Encoding.UTF8.GetString(messageData)}", WebDriverBiDiLogLevel.Debug);
                            }

                            await this.OnDataReceived.NotifyObserversAsync(new ConnectionDataReceivedEventArgs(messageData));
                        }

                        startIndex = i + 1;
                    }
                }

                // If there's remaining data after the last null terminator (or no null found), buffer it
                if (startIndex < bytesRead)
                {
                    messageBuffer.Write(readBuffer, startIndex, bytesRead - startIndex);
                }
            }

            await this.LogAsync($"Ending pipe receive loop");
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledException is normal upon task/token cancellation, so disregard it
        }
        catch (IOException e)
        {
            await this.LogAsync($"Unexpected error during receive of data: {e.Message}");
            await this.OnConnectionError.NotifyObserversAsync(new ConnectionErrorEventArgs(e));
        }
        catch (ObjectDisposedException e)
        {
            await this.LogAsync($"Unexpected error during receive of data: {e.Message}");
            await this.OnConnectionError.NotifyObserversAsync(new ConnectionErrorEventArgs(e));
        }
        finally
        {
            messageBuffer.Dispose();
        }
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Connection"/>.
    /// Override this method in derived classes to add custom async cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected override async ValueTask DisposeAsyncCore()
    {
        if (this.SetDisposed())
        {
            try
            {
                if (this.IsActive)
                {
                    await this.StopAsync();
                }
            }
            catch (Exception ex)
            {
                await this.LogAsync($"Unexpected exception during disposal: {ex.Message}", WebDriverBiDiLogLevel.Warn);
            }

            // Special note: We don't dispose the external process here, as it's owned
            // by the caller and may be used across multiple connection sessions.
            // Disposing it here could cause ObjectDisposedExceptions in the caller
            // if they attempt to use the process after the connection is disposed.
            this.dataSendSemaphore.Dispose();
            this.connectionTokenSource.Dispose();
            this.pipeToProcess.Dispose();
            this.pipeFromProcess.Dispose();
        }
    }
}
