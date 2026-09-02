// <copyright file="PipeConnection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Buffers;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
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
    private readonly AnonymousPipeServerStream pipeToProcess;
    private readonly AnonymousPipeServerStream pipeFromProcess;
    private readonly IPipeServerProcessProvider processProvider;

    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required
    private int isConnectionActiveTypeSafeFlag = 0;
    private int areConnectionPipesDisposedTypeSafeFlag = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeConnection"/> class.
    /// </summary>
    /// <param name="processProvider">An implementation of <see cref="IPipeServerProcessProvider"/> that provides a <see cref="Process"/> that is able to send and receive messages over pipe connections.</param>
    public PipeConnection(IPipeServerProcessProvider processProvider)
    {
        if (processProvider is null)
        {
            throw new ArgumentNullException(nameof(processProvider), "Pipe server process provider must not be null");
        }

        // PipeDirection.Out means we write to this pipe (browser will read from FD 3)
        this.pipeToProcess = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);

        // PipeDirection.In means we read from this pipe (browser will write to FD 4)
        this.pipeFromProcess = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);

        this.processProvider = processProvider;
    }

    /// <summary>
    /// Gets a value indicating whether this connection is active.
    /// </summary>
    /// <remarks>
    /// The returned value is a point-in-time snapshot. Because the pipe server process is owned
    /// by an external caller through<see cref="IPipeServerProcessProvider"/>, the process may
    /// exit or be disposed between this check and any subsequent I/O call. If the owning process
    /// has already been disposed, this property returns <see langword="false"/> rather than
    /// propagating the resulting <see cref="InvalidOperationException"/>. Transient races where
    /// the process exits after <see cref="IsActive"/> returns <see langword="true"/> are surfaced
    /// by <see cref="Connection.SendDataAsync"/> as <see cref="WebDriverBiDiConnectionException"/>.
    /// </remarks>
    public override bool IsActive => this.IsConnectionActive && IsProcessRunning(this.processProvider.PipeServerProcess);

    /// <summary>
    /// Gets a value indicating the type of data transport used by this connection, in this case, pipes.
    /// </summary>
    public override ConnectionKind ConnectionKind => ConnectionKind.Pipes;

    /// <summary>
    /// Gets the handle used for sending data to the external process.
    /// </summary>
    public string ReadPipeHandle => this.AreConnectionPipesDisposed ? string.Empty : this.pipeToProcess.GetClientHandleAsString();

    /// <summary>
    /// Gets the handle used for receiving data from the external process.
    /// </summary>
    public string WritePipeHandle => this.AreConnectionPipesDisposed ? string.Empty : this.pipeFromProcess.GetClientHandleAsString();

    /// <summary>
    /// Gets or sets a value indicating whether the local copies of pipe handles have been disposed.
    /// </summary>
    protected bool AreConnectionPipesDisposed
    {
        get
        {
            return Interlocked.CompareExchange(ref this.areConnectionPipesDisposedTypeSafeFlag, 0, 0) == 1;
        }

        set
        {
            int flagValue = value ? 1 : 0;
            Interlocked.Exchange(ref this.areConnectionPipesDisposedTypeSafeFlag, flagValue);
        }
    }

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
    /// <exception cref="WebDriverBiDiConnectionException">
    /// Thrown when the external application is not yet running, the pipe connection is already
    /// connected, or the receive loop from a previous session is still running after a bounded
    /// wait (see the remarks on <see cref="StopAsync(CancellationToken)"/>).
    /// </exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public override async Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        Process? pipeServerProcess = this.processProvider.PipeServerProcess;
        if (pipeServerProcess is null)
        {
            throw new WebDriverBiDiConnectionException("External process has not been set. Call SetExternalProcess before StartAsync.");
        }

        if (this.IsDisposed)
        {
            throw new WebDriverBiDiConnectionException("The pipes have been disposed; the connection cannot be restarted after disposal.");
        }

        if (!IsProcessRunning(pipeServerProcess))
        {
            throw new WebDriverBiDiConnectionException("External process has already exited or been disposed; cannot start pipe connection.");
        }

        if (this.IsConnectionActive)
        {
            throw new WebDriverBiDiConnectionException($"The pipe connection is already active for {this.ConnectionString}; call the Stop method to disconnect before calling Start");
        }

        // StopAsync may have abandoned the previous session's receive loop still blocked in
        // a pipe read that did not honor cancellation (see the remarks on StopAsync).
        // Starting a second loop over the same pipe would interleave the two loops' reads
        // arbitrarily, corrupting message framing and routing stale data into the new
        // session. Give a loop that is still unwinding a bounded chance to finish, and
        // refuse to start while it runs.
        await this.WaitForReceiveTaskCompletionAsync().ConfigureAwait(false);
        if (this.DataReceiveTask is not null && !this.DataReceiveTask.IsCompleted)
        {
            throw new WebDriverBiDiConnectionException("Cannot start the pipe connection: the receive loop from a previous session has not yet completed, most likely because it is blocked in a pipe read that did not honor cancellation; the connection cannot be restarted until that read completes");
        }

        await this.LogAsync($"Starting pipe connection: {connectionString}").ConfigureAwait(false);

        // Dispose client handles in parent process only on first start - the external process has inherited them
        if (!this.AreConnectionPipesDisposed)
        {
            this.pipeToProcess.DisposeLocalCopyOfClientHandle();
            this.pipeFromProcess.DisposeLocalCopyOfClientHandle();
            this.AreConnectionPipesDisposed = true;
        }

        // Create a new cancellation token source for this connection session
        this.ResetConnectionCancellation();

        this.ConnectionString = connectionString;
        this.IsConnectionActive = true;

        this.StartDataReceiveTask();

        await this.LogAsync("Pipe connection started").ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously stops communication with the remote end of this connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// Waiting for the receive loop to finish is bounded by <see cref="Connection.ShutdownTimeout"/>.
    /// Unlike <see cref="WebSocketConnection"/>, where cancelling the connection's token reliably aborts
    /// a pending receive, cancelling the pipe connection's token does not guarantee that an in-progress
    /// pipe read unblocks promptly on every supported target framework. If the receive loop does not
    /// finish within the timeout, a warning is logged and this method returns anyway; the receive task
    /// continues running in the background until the pipe unblocks on its own. While that abandoned
    /// loop is still running, <see cref="StartAsync(string, CancellationToken)"/> refuses to begin a
    /// new session, and any data the abandoned read eventually returns is discarded rather than
    /// dispatched.
    /// </remarks>
    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.LogAsync("Closing pipe connection").ConfigureAwait(false);

        if (!this.IsConnectionActive)
        {
            await this.LogAsync("Pipe connection already closed").ConfigureAwait(false);
            return;
        }

        // Signal cancellation to stop the receive loop, then wait for the receive task
        // to complete.
        this.CancelConnection();
        await this.WaitForReceiveTaskCompletionAsync().ConfigureAwait(false);

        this.IsConnectionActive = false;
        this.ConnectionString = string.Empty;

        await this.LogAsync("Pipe connection closed").ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends data to the underlying pipe of this connection.
    /// </summary>
    /// <param name="messageBuffer">The buffer containing the data to be sent to the remote end of this connection via the pipe.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when an exception is encountered sending data to the pipe.</exception>
    protected override async Task SendConnectionDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        try
        {
            await this.WritePipeDataAsync(messageBuffer, cancellationToken).ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            throw new WebDriverBiDiConnectionException($"An error occurred while sending data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Asynchronously writes data to the underlying pipe of this connection.
    /// </summary>
    /// <param name="messageBuffer">The data to write to the pipe.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task WritePipeDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        // Write the data followed by a null terminator
#if NET5_0_OR_GREATER
        await this.pipeToProcess.WriteAsync(messageBuffer, cancellationToken).ConfigureAwait(false);
        await this.pipeToProcess.WriteAsync(new ReadOnlyMemory<byte>([0]), cancellationToken).ConfigureAwait(false);
#else
        byte[] data = messageBuffer.ToArray();
        await this.pipeToProcess.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        await this.pipeToProcess.WriteAsync(new byte[] { 0 }, 0, 1, cancellationToken).ConfigureAwait(false);
#endif
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
        return await this.pipeFromProcess.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously receives data from the remote end of this connection.
    /// Messages are expected to be null-terminated as per the WebDriver BiDi pipe protocol.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async Task ReceiveDataAsync()
    {
        CancellationToken connectionCancellationToken = this.ConnectionCancellationToken;
        using MessageBuffer messageBuffer = new();
        using IMemoryOwner<byte> receivedDataBufferOwner = MemoryPool<byte>.Shared.Rent(this.BufferSize);
        try
        {
            // MemoryPool<byte>.Shared is backed by ArrayPool, so TryGetArray always succeeds here.
            MemoryMarshal.TryGetArray(receivedDataBufferOwner.Memory.Slice(0, this.BufferSize), out ArraySegment<byte> readSegment);
            byte[] readArray = readSegment.Array!;
            while (!connectionCancellationToken.IsCancellationRequested)
            {
                int bytesRead = await this.ReadPipeDataAsync(readArray, 0, this.BufferSize, connectionCancellationToken).ConfigureAwait(false);
                if (connectionCancellationToken.IsCancellationRequested)
                {
                    // The session was canceled while the read was blocked (pipe reads do not
                    // reliably honor cancellation on every target framework). Data returned
                    // by such a read belongs to no session; dispatching it would deliver
                    // stale bytes to observers of a connection that has been stopped.
                    break;
                }

                if (bytesRead == 0)
                {
                    // Pipe closed. The remote end can reach end-of-file while its process is
                    // still running, so the process check in IsActive cannot be relied upon
                    // to report the connection as inactive. Clear the flag here, before any
                    // observers are notified, so that a disconnection handler (and a
                    // subsequent Transport.ConnectAsync, which skips Connection.StartAsync
                    // for an active connection) sees the connection as no longer active.
                    this.IsConnectionActive = false;
                    await this.LogAsync("Pipe closed by remote end").ConfigureAwait(false);
                    break;
                }

                // Process the received data, looking for null terminators
                int startIndex = 0;
                for (int i = 0; i < bytesRead; i++)
                {
                    if (readArray[i] == 0)
                    {
                        // Found a null terminator - complete the message. The accumulator's pooled
                        // buffer becomes the message buffer directly (a message contained entirely in
                        // this read is copied exactly once, from the read buffer into pooled memory),
                        // and the IncomingMessage built from it returns the buffer to the pool on disposal.
                        messageBuffer.Append(readArray.AsSpan(startIndex, i - startIndex));
                        if (messageBuffer.HasData)
                        {
                            IMemoryOwner<byte> messageOwner = messageBuffer.TakeOwnership(out int messageLength);

                            if (this.OnLogMessage.CurrentObserverCount > 0)
                            {
#if NET5_0_OR_GREATER
                                await this.LogAsync($"RECV <<< {Encoding.UTF8.GetString(messageOwner.Memory.Span.Slice(0, messageLength))}", WebDriverBiDiLogLevel.Trace).ConfigureAwait(false);
#else
                                await this.LogAsync($"RECV <<< {Encoding.UTF8.GetString(messageOwner.Memory.Slice(0, messageLength).ToArray())}", WebDriverBiDiLogLevel.Trace).ConfigureAwait(false);
#endif
                            }

                            await this.InvocableConnectionDataReceivedObservableEvent.InvokeNotifyObserversAsync(new ConnectionDataReceivedEventArgs(messageOwner, messageLength)).ConfigureAwait(false);
                        }

                        startIndex = i + 1;
                    }
                }

                // If there's remaining data after the last null terminator (or no null found), buffer it
                messageBuffer.Append(readArray.AsSpan(startIndex, bytesRead - startIndex));
            }

            await this.LogAsync($"Ending pipe receive loop").ConfigureAwait(false);

            // If the loop exited without cancellation, the remote end closed the connection gracefully.
            if (!connectionCancellationToken.IsCancellationRequested)
            {
                await this.InvocableRemoteDisconnectedObservableEvent.InvokeNotifyObserversAsync(new ConnectionDisconnectedEventArgs()).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledException is normal upon task/token cancellation, so disregard it.
            // The flag is deliberately left alone here: cancellation means StopAsync is running,
            // and StopAsync owns clearing the flag on that path.
        }
        catch (IOException e)
        {
            // The receive loop is exiting and no further data can be received, so the connection
            // is no longer active regardless of the state of the pipe or the server process.
            // Clear the flag before notifying observers, for the same reason as the end-of-file
            // path above.
            this.IsConnectionActive = false;
            await this.LogAsync($"Unexpected error during receive of data: {e.Message}").ConfigureAwait(false);
            await this.InvocableConnectionErrorObservableEvent.InvokeNotifyObserversAsync(new ConnectionErrorEventArgs(e)).ConfigureAwait(false);
        }
        catch (ObjectDisposedException e)
        {
            this.IsConnectionActive = false;
            await this.LogAsync($"Unexpected error during receive of data: {e.Message}").ConfigureAwait(false);
            await this.InvocableConnectionErrorObservableEvent.InvokeNotifyObserversAsync(new ConnectionErrorEventArgs(e)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // If the observer for OnDataReceived throws an unhandled exception, we will capture
            // that here. This is important because otherwise the loop would stop silently, which
            // is a separate case than the simple case of no further data being received. For
            // pending commands, this would look like a command that never returns a response
            // rather than the loop ending due to the observer exception.
            this.IsConnectionActive = false;
            await this.LogAsync($"Unexpected error processing received data: {e.Message}", WebDriverBiDiLogLevel.Error).ConfigureAwait(false);
            await this.InvocableConnectionErrorObservableEvent.InvokeNotifyObserversAsync(new ConnectionErrorEventArgs(e)).ConfigureAwait(false);
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
                    await this.StopAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await this.LogAsync($"Unexpected exception during disposal: {ex.Message}", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            }

            // Special note: We don't dispose the external process here, as it's owned
            // by the caller and may be used across multiple connection sessions.
            // Disposing it here could cause ObjectDisposedExceptions in the caller
            // if they attempt to use the process after the connection is disposed.
            this.pipeToProcess.Dispose();
            this.pipeFromProcess.Dispose();
        }
    }

    private static bool IsProcessRunning(Process? process)
    {
        if (process is null)
        {
            return false;
        }

        try
        {
            return !process.HasExited;
        }
        catch (InvalidOperationException)
        {
            // The process reference has been disposed by its owner;
            // treat as not running.
            return false;
        }
    }
}
