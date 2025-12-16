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
public class PipeConnection : Connection
{
    private readonly SemaphoreSlim dataSendSemaphore = new(1, 1);
    private readonly AnonymousPipeServerStream pipeToProcess;
    private readonly AnonymousPipeServerStream pipeFromProcess;
    private Task? dataReceiveTask;
    private CancellationTokenSource connectionTokenSource = new();
    private Process? externalProcess;
    private bool isActive;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeConnection"/> class.
    /// </summary>
    public PipeConnection()
    {
        // PipeDirection.Out means WE write to this pipe (browser will read from FD 3)
        this.pipeToProcess = new AnonymousPipeServerStream(
            PipeDirection.Out,
            HandleInheritability.Inheritable);

        // PipeDirection.In means WE read from this pipe (browser will write to FD 4)
        this.pipeFromProcess = new AnonymousPipeServerStream(
            PipeDirection.In,
            HandleInheritability.Inheritable);
    }

    /// <summary>
    /// Gets a value indicating whether this connection is active.
    /// </summary>
    public override bool IsActive => this.isActive && this.externalProcess is not null && !this.externalProcess.HasExited;

    /// <summary>
    /// Gets a value indicating the type of data transport used by this connection, in this case, pipes.
    /// </summary>
    public override ConnectionType ConnectionType => ConnectionType.Pipes;

    /// <summary>
    /// Gets the handle used for sending data to the external process.
    /// </summary>
    public string ReadPipeHandle => this.pipeToProcess.GetClientHandleAsString();

    /// <summary>
    /// Gets the handle used for receiving data from the external process.
    /// </summary>
    public string WritePipeHandle => this.pipeFromProcess.GetClientHandleAsString();

    /// <summary>
    /// Sets the external process that this connection will communicate with.
    /// This must be called before StartAsync.
    /// </summary>
    /// <param name="process">The browser process.</param>
    public void SetExternalProcess(Process process)
    {
        this.externalProcess = process;
    }

    /// <summary>
    /// Asynchronously starts communication with the remote end of this connection.
    /// CreatePipes and SetBrowserProcess must be called before this method.
    /// </summary>
    /// <param name="connectionString">A description string for this connection (e.g., "pipe://chromium").</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the external process is not set.</exception>
    /// <exception cref="TimeoutException">Thrown when the connection is not established within the startup timeout.</exception>
    public override async Task StartAsync(string connectionString)
    {
        if (this.externalProcess is null)
        {
            throw new WebDriverBiDiException("External process has not been set. Call SetExternalProcess before StartAsync.");
        }

        if (this.isActive)
        {
            throw new WebDriverBiDiException($"The pipe connection is already active for {this.ConnectionString}; call the Stop method to disconnect before calling Start");
        }

        await this.LogAsync($"Starting pipe connection: {connectionString}");

        // Dispose client handles in parent process - the external process has inherited them
        this.pipeToProcess.DisposeLocalCopyOfClientHandle();
        this.pipeFromProcess.DisposeLocalCopyOfClientHandle();

        this.connectionTokenSource = new CancellationTokenSource();
        this.ConnectionString = connectionString;
        this.isActive = true;

        // Start the receive loop
        this.dataReceiveTask = Task.Run(async () => await this.ReceiveDataAsync().ConfigureAwait(false));

        await this.LogAsync("Pipe connection started");
    }

    /// <summary>
    /// Asynchronously stops communication with the remote end of this connection.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override async Task StopAsync()
    {
        await this.LogAsync("Closing pipe connection");

        if (!this.isActive)
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

        // Dispose the pipes
        this.pipeToProcess.Dispose();
        this.pipeFromProcess.Dispose();

        this.isActive = false;
        this.ConnectionString = string.Empty;

        await this.LogAsync("Pipe connection closed");
    }

    /// <summary>
    /// Asynchronously sends data to the remote end of this connection.
    /// Data is sent as a null-terminated message as per the WebDriver BiDi pipe protocol.
    /// </summary>
    /// <param name="data">The data to be sent to the remote end of this connection.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the connection is not active.</exception>
    public override async Task SendDataAsync(byte[] data)
    {
        if (!this.IsActive)
        {
            throw new WebDriverBiDiException("The pipe connection has not been initialized; you must call the Start method before sending data");
        }

        // Only one send operation at a time
        if (!await this.dataSendSemaphore.WaitAsync(this.DataTimeout).ConfigureAwait(false))
        {
            throw new WebDriverBiDiTimeoutException("Timed out waiting to access pipe for sending; only one send operation is permitted at a time.");
        }

        try
        {
            if (this.OnLogMessage.CurrentObserverCount > 0)
            {
                await this.LogAsync($"SEND >>> {Encoding.UTF8.GetString(data)}", WebDriverBiDiLogLevel.Debug);
            }

            await this.SendPipeDataAsync(data);
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
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual async Task SendPipeDataAsync(byte[] messageBuffer)
    {
        // Write the data followed by a null terminator
        await this.pipeToProcess.WriteAsync(messageBuffer, 0, messageBuffer.Length).ConfigureAwait(false);
        byte[] nullTerminator = [0];
        await this.pipeToProcess.WriteAsync(nullTerminator, 0, 1).ConfigureAwait(false);
        await this.pipeToProcess.FlushAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously receives data from the remote end of this connection.
    /// Messages are expected to be null-terminated as per the WebDriver BiDi pipe protocol.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async Task ReceiveDataAsync()
    {
        CancellationToken cancellationToken = this.connectionTokenSource.Token;
        try
        {
            MemoryStream messageBuffer = new();
            byte[] readBuffer = new byte[this.BufferSize];

            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await this.pipeFromProcess.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationToken).ConfigureAwait(false);
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
    }
}
