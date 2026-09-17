// <copyright file="PipeConnection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Buffers;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;

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
/// <item><description>Two anonymous pipes are created on every platform and their handles are inherited by the browser process</description></item>
/// <item><description>On Unix systems the browser reads from file descriptor 3 and writes to file descriptor 4; on Windows it receives the inherited handles</description></item>
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
    /// Gets a value indicating the type of data transport used by this connection, in this case, pipes.
    /// </summary>
    public override ConnectionKind ConnectionKind => ConnectionKind.Pipes;

    /// <summary>
    /// Gets the handle the external process reads from, through which this connection sends it data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The names of the two handles take the external process's point of view, as the arguments of a browser's
    /// pipe-based remote debugging option do (Chromium's <c>--remote-debugging-io-pipes=&lt;read&gt;,&lt;write&gt;</c>).
    /// </para>
    /// <para>
    /// Returns an empty string once the connection has started: the first start disposes this process's local
    /// copy of the client handle, which the external process has inherited by then. Read the handle before
    /// starting the connection.
    /// </para>
    /// </remarks>
    public string ReadPipeHandle => this.AreConnectionPipesDisposed ? string.Empty : this.pipeToProcess.GetClientHandleAsString();

    /// <summary>
    /// Gets the handle the external process writes to, through which this connection receives its data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The names of the two handles take the external process's point of view; see <see cref="ReadPipeHandle"/>.
    /// </para>
    /// <para>
    /// Returns an empty string once the connection has started: the first start disposes this process's local
    /// copy of the client handle, which the external process has inherited by then. Read the handle before
    /// starting the connection.
    /// </para>
    /// </remarks>
    public string WritePipeHandle => this.AreConnectionPipesDisposed ? string.Empty : this.pipeFromProcess.GetClientHandleAsString();

    /// <summary>
    /// Gets a value indicating whether the pipes to the external process are open.
    /// </summary>
    /// <remarks>
    /// The returned value is a point-in-time snapshot. Because the pipe server process is owned
    /// by an external caller through <see cref="IPipeServerProcessProvider"/>, the process may
    /// exit or be disposed between this check and any subsequent I/O call. If the owning process
    /// has already been disposed, this property returns <see langword="false"/> rather than
    /// propagating the resulting <see cref="InvalidOperationException"/>. Transient races where
    /// the process exits after <see cref="Connection.IsActive"/> returns <see langword="true"/> are surfaced
    /// by <see cref="Connection.SendDataAsync"/> as <see cref="WebDriverBiDiConnectionException"/>.
    /// </remarks>
    protected override bool IsConnectionOpen => this.IsConnectionActive && IsProcessRunning(this.processProvider.PipeServerProcess);

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
    /// Asynchronously makes the pipes to the external process ready to carry a session.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">
    /// Thrown when the external process has not been set, or is not running.
    /// </exception>
    /// <remarks>
    /// A pipe connection accepts any connection string -- the pipes it uses are its own, and the string
    /// only names the session -- so it does not override
    /// <see cref="Connection.ResolveConnectionString"/>, and there is nothing for this method to
    /// interpret. The pipes themselves are created with this connection and inherited by the external
    /// process, so there is no connect operation to perform and nothing here is cancellable;
    /// <paramref name="cancellationToken"/> has already been observed by
    /// <see cref="Connection.StartAsync"/> before this method is called.
    /// </remarks>
    protected override Task StartConnectionAsync(CancellationToken cancellationToken)
    {
        Process? pipeServerProcess = this.processProvider.PipeServerProcess;
        if (pipeServerProcess is null)
        {
            throw new WebDriverBiDiConnectionException("External process has not been set. Make sure IPipeServerProcessProvider.PipeServerProcess is valid before StartAsync.");
        }

        if (!IsProcessRunning(pipeServerProcess))
        {
            throw new WebDriverBiDiConnectionException("External process has already exited or been disposed; cannot start pipe connection.");
        }

        // Dispose client handles in parent process only on first start - the external process has inherited them
        if (!this.AreConnectionPipesDisposed)
        {
            this.pipeToProcess.DisposeLocalCopyOfClientHandle();
            this.pipeFromProcess.DisposeLocalCopyOfClientHandle();
            this.AreConnectionPipesDisposed = true;
        }

        this.IsConnectionActive = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks the pipe connection as no longer carrying a session.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// A pipe connection has no shutdown exchange with the remote end: the pipes are torn down by
    /// cancelling the connection, which <see cref="Connection.StopAsync"/> does after this method
    /// returns. All this method does is record that the session is over, before the receive loop is
    /// canceled, so that nothing sees the connection as active while it unwinds.
    /// </para>
    /// <para>
    /// Cancelling the connection's token does not guarantee that an in-progress pipe read unblocks
    /// promptly on every supported target framework, so the wait for the receive loop that
    /// <see cref="Connection.StopAsync"/> performs is more often reached here than it is for a
    /// <see cref="WebSocketConnection"/>. Any data an abandoned read eventually returns is discarded
    /// rather than dispatched.
    /// </para>
    /// </remarks>
    protected override async Task StopConnectionAsync(CancellationToken cancellationToken)
    {
        if (!this.IsConnectionActive)
        {
            // The connection was never started, or the receive loop has already recorded the pipe as
            // closed by the remote end, so there is nothing left for this method to record.
            await this.LogAsync("Pipe connection is not active", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
            return;
        }

        this.IsConnectionActive = false;
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
    /// <remarks>
    /// <para>
    /// The message and the null terminator that frames it are written as a single operation, so that
    /// cancellation cannot separate them. Written as two cancellable writes, a token canceled after the
    /// first one completes would leave an unterminated message in the pipe; the remote end would then read
    /// that message and the next one as a single malformed message, and every message after it would be
    /// framed one boundary out of step. Nothing later in the session can repair that, and the caller
    /// receives only the cancellation, so the corruption would surface as unrelated failures.
    /// </para>
    /// <para>
    /// Cancellation is therefore honored up to the point the first byte is written, and not after it: the
    /// flush that follows the write uses <see cref="CancellationToken.None"/>, because by then the bytes
    /// are already committed to the stream.
    /// </para>
    /// </remarks>
    protected virtual async Task WritePipeDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        // Copy the message and its null terminator into one buffer so that a single write emits the whole
        // frame. The rented array is usually longer than the frame, so every use below is bounded by
        // frameLength rather than by the array's own length.
        int frameLength = messageBuffer.Length + 1;
        byte[] frame = ArrayPool<byte>.Shared.Rent(frameLength);
        try
        {
            messageBuffer.CopyTo(frame);
            frame[messageBuffer.Length] = 0;
            await this.WriteToPipeAsync(frame, 0, frameLength, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(frame);
        }

        await this.pipeToProcess.FlushAsync(CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously writes bytes to the underlying pipe of this connection.
    /// </summary>
    /// <param name="buffer">The buffer containing the bytes to write.</param>
    /// <param name="offset">The offset in the buffer at which the bytes to write begin.</param>
    /// <param name="count">The number of bytes to write.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This is the counterpart of <see cref="ReadPipeDataAsync"/> for the outbound direction, and is the
    /// single point at which bytes reach the pipe. <see cref="WritePipeDataAsync"/> calls it exactly once
    /// per message, with the message and its null terminator already assembled into one buffer, which is
    /// what keeps cancellation from splitting a frame.
    /// </remarks>
    protected virtual async Task WriteToPipeAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
#if NET5_0_OR_GREATER
        await this.pipeToProcess.WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).ConfigureAwait(false);
#else
        await this.pipeToProcess.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
#endif
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
                    // still running, so the process check in IsConnectionOpen cannot be relied upon
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
                        // Found a null terminator; the bytes accumulated since the previous one
                        // complete a message, and the terminator itself is not part of it. A message
                        // contained entirely in this read is copied exactly once, from the read
                        // buffer into the accumulator's pooled memory. A zero-length message (two
                        // adjacent terminators) leaves the accumulator empty, and notifying with an
                        // empty accumulator delivers nothing, so no guard is needed here.
                        messageBuffer.Append(readArray.AsSpan(startIndex, i - startIndex));
                        await this.NotifyDataReceivedObserverAsync(messageBuffer).ConfigureAwait(false);
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
                await this.NotifyRemoteDisconnectedObserversAsync().ConfigureAwait(false);
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
            await this.NotifyConnectionErrorObserversAsync($"Unexpected error during receive of data: {e.Message}", e).ConfigureAwait(false);
        }
        catch (ObjectDisposedException e)
        {
            this.IsConnectionActive = false;
            await this.NotifyConnectionErrorObserversAsync($"Unexpected error during receive of data: {e.Message}", e).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // If the observer for OnDataReceived throws an unhandled exception, we will capture
            // that here. This is important because otherwise the loop would stop silently, which
            // is a separate case than the simple case of no further data being received. For
            // pending commands, this would look like a command that never returns a response
            // rather than the loop ending due to the observer exception.
            this.IsConnectionActive = false;
            await this.NotifyConnectionErrorObserversAsync($"Unexpected error processing received data: {e.Message}", e).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Connection"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    /// <remarks>
    /// Special note: We don't dispose the external process here, as it's owned by the caller and may be
    /// used across multiple connection sessions. Disposing it here could cause
    /// <see cref="ObjectDisposedException"/> in the caller if they attempt to use the process after the
    /// connection is disposed.
    /// </remarks>
    protected override ValueTask DisposeAsyncCore()
    {
        this.pipeToProcess.Dispose();
        this.pipeFromProcess.Dispose();
        return default;
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
