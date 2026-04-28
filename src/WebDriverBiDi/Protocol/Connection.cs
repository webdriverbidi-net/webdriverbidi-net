// <copyright file="Connection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Represents a connection to a WebDriver Bidi remote end.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Connection"/> class is an abstract base class that defines the contract for
/// transport-layer communication with a browser. It is wrapped by the <see cref="Transport"/> class,
/// which handles protocol-level concerns like JSON serialization and command/response correlation.
/// </para>
/// <para>
/// Most users will never need to interact with <see cref="Connection"/> objects directly.
/// The <see cref="BiDiDriver"/> class manages connections automatically. Custom connection
/// implementations are only needed for specialized transport mechanisms.
/// </para>
/// <para>
/// Available implementations:
/// <list type="bullet">
/// <item><term><see cref="WebSocketConnection"/></term><description>Standard WebSocket transport (recommended for all scenarios)</description></item>
/// <item><term><see cref="PipeConnection"/></term><description>Anonymous pipes transport (specialized for high-performance local Chromium automation)</description></item>
/// </list>
/// </para>
/// <para>
/// Thread safety: Connection implementations use internal synchronization to ensure thread-safe operation.
/// Multiple threads can safely call <see cref="SendDataAsync"/> concurrently. The <see cref="Transport"/>
/// class that wraps connections provides additional synchronization for <see cref="StartAsync"/> and
/// <see cref="StopAsync"/> operations.
/// </para>
/// </remarks>
public abstract class Connection : IAsyncDisposable
{
    /// <summary>
    /// Gets the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Connection";

    private const string DataReceivedEventName = "connection.dataReceived";
    private const string LogMessageEventName = "connection.logMessage";
    private const string ConnectionErrorEventName = "connection.connectionError";
    private const string RemoteDisconnectedEventName = "connection.remoteDisconnected";
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required
    private int isDisposedFlag;

    /// <summary>
    /// Gets a value indicating whether this connection is active.
    /// </summary>
    public abstract bool IsActive { get; }

    /// <summary>
    /// Gets a value indicating the type of data transport used by this connection.
    /// </summary>
    public abstract ConnectionType ConnectionType { get; }

    /// <summary>
    /// Gets the buffer size for communication used by this connection.
    /// </summary>
    public int BufferSize { get; } = Convert.ToInt32(Math.Pow(2, 20));

    /// <summary>
    /// Gets or sets the string containing data about which the connection is connected.
    /// For a WebSocket connection, this is its URL. For a named pipe connection, it is
    /// the name of the pipe.
    /// </summary>
    public string ConnectionString { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the timeout to wait before throwing an error when starting up the connection.
    /// </summary>
    public TimeSpan StartupTimeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Gets or sets the value of the timeout to wait before throwing an error when shutting down the connection.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Gets or sets the value of the timeout to wait for exclusive access when sending to or receiving data from the ClientWebSocket.
    /// </summary>
    public TimeSpan DataTimeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Gets an observable event that notifies when data is received from this connection.
    /// </summary>
    public ObservableEvent<ConnectionDataReceivedEventArgs> OnDataReceived { get; } = new(DataReceivedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a communication error occurs on this connection.
    /// </summary>
    public ObservableEvent<ConnectionErrorEventArgs> OnConnectionError { get; } = new(ConnectionErrorEventName);

    /// <summary>
    /// Gets an observable event that notifies when the remote end gracefully closes this connection.
    /// </summary>
    public ObservableEvent<ConnectionDisconnectedEventArgs> OnRemoteDisconnected { get; } = new(RemoteDisconnectedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a log message is written.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; } = new(LogMessageEventName);

    /// <summary>
    /// Gets a value indicating whether this connection has been disposed.
    /// </summary>
    protected bool IsDisposed => Interlocked.CompareExchange(ref this.isDisposedFlag, 0, 0) == 1;

    /// <summary>
    /// Asynchronously starts communication with the remote end of this connection.
    /// </summary>
    /// <param name="connectionString">The connection string used to connect to the remote end.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public abstract Task StartAsync(string connectionString, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously stops communication with the remote end of this connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public abstract Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously sends data to the remote end of this connection.
    /// </summary>
    /// <param name="data">The data to be sent to the remote end of this connection.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public abstract Task SendDataAsync(byte[] data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Connection"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously receives data from the remote end of this connection.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected abstract Task ReceiveDataAsync();

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Connection"/>.
    /// Override this method in derived classes to add custom async cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected abstract ValueTask DisposeAsyncCore();

    /// <summary>
    /// Marks this <see cref="Connection"/> as disposed. Use this method to ensure
    /// thread-safe operations for setting object being disposed.
    /// </summary>
    /// <returns><see langword="true"/> if the object was not already disposed before calling this method; otherwise, <see langword="false"/>.</returns>
    protected bool SetDisposed()
    {
        return Interlocked.Exchange(ref this.isDisposedFlag, 1) == 0;
    }

    /// <summary>
    /// Asynchronously raises a logging event at the specified log level.
    /// </summary>
    /// <param name="message">The log message to raise in the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected async Task LogAsync(string message)
    {
        await this.LogAsync(message, WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously raises a logging event at the specified log level.
    /// </summary>
    /// <param name="message">The log message to raise in the event.</param>
    /// <param name="level">The <see cref="WebDriverBiDiLogLevel"/> at which to raise the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected async Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.OnLogMessage.NotifyObserversAsync(new LogMessageEventArgs(message, level, LoggerComponentName)).ConfigureAwait(false);
    }
}
