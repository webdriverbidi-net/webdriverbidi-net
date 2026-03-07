// <copyright file="Transport.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The transport object used for serializing and deserializing JSON data used in the WebDriver Bidi protocol.
/// It uses a <see cref="Connection"/> object to communicate with the remote end, and does no further processing
/// of the objects serialized or deserialized. Consumers of this class are expected to handle things like awaiting
/// the response of a WebDriver BiDi command message.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Message Queue Architecture:</strong>
/// This transport uses an unbounded <see cref="System.Threading.Channels.Channel{T}"/> for message processing.
/// Messages received from the connection are queued and processed sequentially by a dedicated reader task.
/// </para>
/// <para>
/// <strong>Memory Considerations:</strong>
/// The unbounded queue means there is no limit on the number of messages that can be buffered if they
/// arrive faster than they can be processed. In typical usage, message processing is fast enough that
/// queue depth remains minimal. However, in high-throughput scenarios (e.g., thousands of rapid events
/// or very slow event handlers), memory consumption could grow significantly.
/// </para>
/// <para>
/// <strong>Performance Characteristics:</strong>
/// The single-reader, single-writer queue design provides optimal throughput for the typical case where
/// messages arrive sequentially and are processed quickly. Event handlers that perform slow operations
/// (I/O, CPU-intensive work) should use <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/>
/// to avoid blocking the message processing thread.
/// </para>
/// <para>
/// <strong>Monitoring Queue Behavior:</strong>
/// There is currently no built-in mechanism to monitor queue depth. If you suspect message backlog issues,
/// consider:
/// <list type="bullet">
/// <item><description>Using <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/> for all event handlers that perform I/O or take more than a few milliseconds</description></item>
/// <item><description>Monitoring memory usage of your process during high-traffic operations</description></item>
/// <item><description>Reducing the frequency of subscribed events if not all are needed</description></item>
/// <item><description>Implementing throttling or filtering logic in your event handlers</description></item>
/// </list>
/// </para>
/// </remarks>
public class Transport : IAsyncDisposable
{
    private const string EventReceivedEventName = "transport.eventReceived";
    private const string ErrorReceivedEventName = "transport.errorReceived";
    private const string UnknownMessageReceivedEventName = "transport.unknownMessageReceived";
    private const string LogMessageEventName = "transport.logMessage";

    private readonly JsonSerializerOptions options = new()
    {
        TypeInfoResolver = CreateTypeInfoResolver(),
    };

    private readonly ConcurrentDictionary<string, Type> eventMessageTypes = [];
    private readonly UnhandledErrorCollection unhandledErrors = new();
    private readonly SemaphoreSlim connectDisconnectSemaphore = new(1, 1);
    private readonly ConcurrentDictionary<long, Stopwatch> commandTimings = [];
    private Channel<byte[]> queue = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions()
    {
        SingleReader = true,
        SingleWriter = true,
    });

    private PendingCommandCollection pendingCommands = new();
    private Task messageQueueProcessingTask = Task.CompletedTask;
    private long nextCommandId = 0;
    private int isConnectedTypeSafeFlag = 0;
    private string terminationReason = "Normal shutdown";

    /// <summary>
    /// Initializes a new instance of the <see cref="Transport"/> class.
    /// </summary>
    public Transport()
        : this(new WebSocketConnection())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Transport"/> class with a given command timeout and connection.
    /// </summary>
    /// <param name="connection">The connection used to communicate with the protocol remote end.</param>
    public Transport(Connection connection)
    {
        this.Connection = connection;
        connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        connection.OnConnectionError.AddObserver(this.OnConnectionErrorAsync);
        connection.OnLogMessage.AddObserver(this.OnConnectionLogMessageAsync);
    }

    /// <summary>
    /// Gets an observable event that notifies when an event is received from the protocol.
    /// </summary>
    public ObservableEvent<EventReceivedEventArgs> OnEventReceived { get; } = new(EventReceivedEventName);

    /// <summary>
    /// Gets an observable event that notifies when an error is received from the protocol
    /// that is not the result of a command execution.
    /// </summary>
    public ObservableEvent<ErrorReceivedEventArgs> OnErrorEventReceived { get; } = new(ErrorReceivedEventName);

    /// <summary>
    /// Gets an observable event that notifies when an unknown message is received from the protocol.
    /// </summary>
    public ObservableEvent<UnknownMessageReceivedEventArgs> OnUnknownMessageReceived { get; } = new(UnknownMessageReceivedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a log message is written.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; } = new(LogMessageEventName);

    /// <summary>
    /// Gets or sets a value indicating how this <see cref="Transport"/> should behave when an
    /// unhandled exception in a handler for a defined protocol is encountered. Defaults to
    /// ignoring exceptions, in which case, those exceptions will never be surfaced to the user.
    /// </summary>
    public TransportErrorBehavior EventHandlerExceptionBehavior { get => this.unhandledErrors.EventHandlerExceptionBehavior; set => this.unhandledErrors.EventHandlerExceptionBehavior = value; }

    /// <summary>
    /// Gets or sets a value indicating how this <see cref="Transport"/> should behave when a
    /// protocol error is encountered, such as invalid JSON or JSON missing required properties.
    /// Defaults to ignoring exceptions, in which case, those exceptions will never be surfaced
    /// to the user.
    /// </summary>
    public TransportErrorBehavior ProtocolErrorBehavior { get => this.unhandledErrors.ProtocolErrorBehavior; set => this.unhandledErrors.ProtocolErrorBehavior = value; }

    /// <summary>
    /// Gets or sets a value indicating how this <see cref="Transport"/> should behave when an
    /// unknown message is encountered, such as valid JSON that does not match any protocol data
    /// structure. Defaults to ignoring exceptions, in which case, those exceptions will never
    /// be surfaced to the user.
    /// </summary>
    public TransportErrorBehavior UnknownMessageBehavior { get => this.unhandledErrors.UnknownMessageBehavior; set => this.unhandledErrors.UnknownMessageBehavior = value; }

    /// <summary>
    /// Gets or sets a value indicating how this <see cref="Transport"/> should behave when an
    /// unexpected error is encountered, meaning an error response received with no corresponding
    /// command. Defaults to ignoring exceptions, in which case, those exceptions will never be
    /// surfaced to the user.
    /// </summary>
    public TransportErrorBehavior UnexpectedErrorBehavior { get => this.unhandledErrors.UnexpectedErrorBehavior; set => this.unhandledErrors.UnexpectedErrorBehavior = value; }

    /// <summary>
    /// Gets or sets the timeout to wait for message processing to complete during shutdown.
    /// If message processing does not complete within this timeout, the shutdown will proceed
    /// without waiting for the remaining processing to finish. The default is 10 seconds.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets a value indicating whether this transport is connected to a connection.
    /// Use this property to ensure thread-safe operations for checking connectivity.
    /// </summary>
    internal bool IsConnected
    {
        get
        {
            return Interlocked.CompareExchange(ref this.isConnectedTypeSafeFlag, 0, 0) == 1;
        }

        set
        {
            int flagValue = value ? 1 : 0;
            Interlocked.Exchange(ref this.isConnectedTypeSafeFlag, flagValue);
        }
    }

    /// <summary>
    /// Gets the ID of the last command to be added.
    /// </summary>
    protected long LastCommandId => this.nextCommandId;

    /// <summary>
    /// Gets the connection used to communicate with the browser.
    /// </summary>
    protected Connection Connection { get; }

    /// <summary>
    /// Asynchronously connects to the remote end web socket.
    /// </summary>
    /// <param name="websocketUri">The URI used to connect to the web socket.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the transport is already connected to a remote end.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task ConnectAsync(string websocketUri, CancellationToken cancellationToken = default)
    {
        await this.AcquireConnectionLockAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.IsConnected)
            {
                throw new WebDriverBiDiConnectionException($"The transport is already connected to {this.Connection.ConnectionString}; you must disconnect before connecting to another URL");
            }

            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening(this.Connection.GetHashCode().ToString(), websocketUri);

            if (!this.pendingCommands.IsAcceptingCommands)
            {
                this.pendingCommands.Dispose();
                this.pendingCommands = new PendingCommandCollection();
            }

            this.queue = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions()
            {
                SingleReader = true,
                SingleWriter = true,
            });

            this.unhandledErrors.ClearUnhandledErrors();

            // Reset the command counter for each connection.
            Interlocked.Exchange(ref this.nextCommandId, 0);
            if (!this.Connection.IsActive)
            {
                // Allow for the possibility of the connection to already being opened.
                await this.Connection.StartAsync(websocketUri, cancellationToken).ConfigureAwait(false);
            }

            // Delaying starting the processing loop until after establishing the connection
            // shouldn't be an issue, as we are using a Channel for processing the data, which
            // should buffer the data until the first read. If the underlying data structure
            // changes, this logic may need to be refactored.
            this.IsConnected = true;
            this.messageQueueProcessingTask = Task.Run(() => this.ReadIncomingMessagesAsync());

            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpened(this.Connection.GetHashCode().ToString(), websocketUri);
            WebDriverBiDiEventSource.RaiseEvent.TransportStarted();
        }
        finally
        {
            this.ReleaseConnectionLock();
        }
    }

    /// <summary>
    /// Asynchronously disconnects from the remote end web socket.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await this.DisconnectAsync(true, cancellationToken);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end.
    /// </summary>
    /// <param name="commandData">The command settings object containing all data required to execute the command.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the command ID is already in use.</exception>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the transport is not connected to a remote end.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task<Command> SendCommandAsync(CommandParameters commandData, CancellationToken cancellationToken = default)
    {
        if (this.unhandledErrors.TryGetExceptions(TransportErrorBehavior.Terminate, out IList<Exception> terminationExceptions))
        {
            await this.DisconnectAsync(false).ConfigureAwait(false);
            throw this.CreateTerminationException(terminationExceptions);
        }

        await this.AcquireConnectionLockAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!this.IsConnected)
            {
                throw new WebDriverBiDiConnectionException("Transport must be connected to a remote end to execute commands.");
            }

            long commandId = this.GetNextCommandId();
            Command command = new(commandId, commandData);
            await this.pendingCommands.AddPendingCommandAsync(command, cancellationToken).ConfigureAwait(false);

            // Start timing and log command sending
            Stopwatch stopwatch = Stopwatch.StartNew();
            this.commandTimings[commandId] = stopwatch;
            WebDriverBiDiEventSource.RaiseEvent.CommandSending(commandId.ToString(), commandData.MethodName);

            byte[] commandJson = this.SerializeCommand(command);
            await this.Connection.SendDataAsync(commandJson, cancellationToken).ConfigureAwait(false);

            WebDriverBiDiEventSource.RaiseEvent.PendingCommandCount(this.pendingCommands.PendingCommandCount);

            return command;
        }
        finally
        {
            this.ReleaseConnectionLock();
        }
    }

    /// <summary>
    /// Cancels a pending command and removes it from the pending command collection.
    /// If the command has already completed or been removed, this method is a safe no-op.
    /// </summary>
    /// <param name="command">The command to cancel.</param>
    public virtual void CancelCommand(Command command)
    {
        command.Cancel();
        this.pendingCommands.RemovePendingCommand(command.CommandId, out _);
    }

    /// <summary>
    /// Registers an additional <see cref="IJsonTypeInfoResolver"/> for JSON serialization
    /// and deserialization. This allows custom types, such as those from user-defined modules,
    /// to be serialized in AOT scenarios where reflection-based serialization is unavailable.
    /// This method must be called before connecting to the remote end.
    /// </summary>
    /// <param name="resolver">The type info resolver to add.</param>
    /// <exception cref="WebDriverBiDiConnectionException">
    /// Thrown if the transport is already connected to a remote end.
    /// </exception>
    public virtual void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver)
    {
        if (this.IsConnected)
        {
            throw new WebDriverBiDiConnectionException("Cannot register a type info resolver after the transport is connected");
        }

        this.options.TypeInfoResolver = JsonTypeInfoResolver.Combine(this.options.TypeInfoResolver, resolver);
    }

    /// <summary>
    /// Registers an event message to be recognized when received from the connection.
    /// </summary>
    /// <typeparam name="T">The type of data to be returned in the event.</typeparam>
    /// <param name="eventName">The name of the event.</param>
    public virtual void RegisterEventMessage<T>(string eventName)
    {
        this.AddEventMessageType(eventName, typeof(EventMessage<T>));
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Transport"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Adds an event message type to the map of known event message types.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="eventMessageType">The type of data to be returned in the event.</param>
    protected virtual void AddEventMessageType(string eventName, Type eventMessageType)
    {
        this.eventMessageTypes[eventName] = eventMessageType;
    }

    /// <summary>
    /// Increments the command ID for the command to be sent.
    /// </summary>
    /// <returns>The command ID for the command to be sent.</returns>
    protected long GetNextCommandId()
    {
        return Interlocked.Increment(ref this.nextCommandId);
    }

    /// <summary>
    /// Serializes a command for transmission across the WebSocket connection.
    /// </summary>
    /// <param name="command">The command to serialize.</param>
    /// <returns>The serialized JSON string representing the command.</returns>
    protected virtual byte[] SerializeCommand(Command command)
    {
        return JsonSerializer.SerializeToUtf8Bytes(command, this.options);
    }

    /// <summary>
    /// Deserializes an incoming message from the WebSocket connection.
    /// </summary>
    /// <param name="messageData">The message data to deserialize.</param>
    /// <returns>A JsonElement representing the parsed message.</returns>
    /// <exception cref="JsonException">
    /// Thrown when there is a syntax error in the incoming JSON.
    /// </exception>
    protected virtual JsonElement DeserializeMessage(byte[] messageData)
    {
        using JsonDocument document = JsonDocument.Parse(messageData);
        return document.RootElement.Clone();
    }

    /// <summary>
    /// Asynchronously disconnects from the remote end web socket.
    /// </summary>
    /// <param name="throwCollectedExceptions"> A value indicating whether to throw the collected exceptions.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual async Task DisconnectAsync(bool throwCollectedExceptions, CancellationToken cancellationToken = default)
    {
        // Fast-path guard: if not connected, there is nothing to disconnect.
        // This check runs before acquiring the semaphore to prevent a deadlock
        // when an event handler processed during shutdown calls SendCommandAsync,
        // which in turn calls DisconnectAsync on the thread that already holds
        // the semaphore.
        if (!this.IsConnected)
        {
            return;
        }

        await this.AcquireConnectionLockAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check the connection state after acquiring the semaphore to prevent
            // race conditions where multiple threads pass the fast-path check simultaneously.
            // If another thread has already disconnected, we can safely return.
            if (!this.IsConnected)
            {
                return;
            }

            // Mark disconnected before performing teardown so that any
            // re-entrant calls (e.g., from event handlers still executing
            // during shutdown) see the transport as disconnected and
            // short-circuit rather than attempting a redundant disconnect.
            WebDriverBiDiEventSource.RaiseEvent.ConnectionClosing(this.Connection.GetHashCode().ToString(), this.terminationReason);
            this.IsConnected = false;

            // Close the pending command collection to further addition of commands,
            // and stop the connection from receiving further communication traffic.
            await this.pendingCommands.CloseAsync().ConfigureAwait(false);
            await this.Connection.StopAsync(cancellationToken).ConfigureAwait(false);

            // Mark the incoming message queue as complete for writing, indicating
            // no further messages will be written to the queue. Existing messages
            // currently in the queue, however should still be processed. Then,
            // wait for the incoming message queue to consume the remaining messages
            // already in the queue. Note that having all items consumed from the
            // queue does not imply that processing of all items has completed; that
            // must be awaited separately.
            this.queue.Writer.Complete();
            await this.queue.Reader.Completion;

            // Clear the pending command collection. This will also cancel any tasks
            // associated with the remaining pending commands. Then wait for the
            // message processor to complete processing of the messages received from
            // the message queue, but with a timeout to prevent hanging if an
            // in-process event handler is stuck.
            this.pendingCommands.Clear();
            using CancellationTokenSource commandDelayCancelTokenSource = new();
            Task completedTask = await Task.WhenAny(this.messageQueueProcessingTask, Task.Delay(this.ShutdownTimeout, commandDelayCancelTokenSource.Token)).ConfigureAwait(false);
            if (completedTask != this.messageQueueProcessingTask)
            {
                await this.LogAsync("Timed out waiting for message processing to complete during shutdown", WebDriverBiDiLogLevel.Warn);
            }
            else
            {
                commandDelayCancelTokenSource.Cancel();
            }

            WebDriverBiDiEventSource.RaiseEvent.ConnectionClosed(this.Connection.GetHashCode().ToString());
            WebDriverBiDiEventSource.RaiseEvent.TransportStopped(this.terminationReason);

            if (throwCollectedExceptions && this.unhandledErrors.TryGetExceptions(TransportErrorBehavior.Collect, out IList<Exception> collectedExceptions))
            {
                throw this.CreateTerminationException(collectedExceptions, TransportErrorBehavior.Collect);
            }
        }
        finally
        {
            this.ReleaseConnectionLock();
        }
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Transport"/>.
    /// Override this method in derived classes to add custom cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (this.IsConnected)
        {
            try
            {
                await this.DisconnectAsync(false).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this.LogAsync($"Unexpected exception during disposal: {ex.Message}", WebDriverBiDiLogLevel.Warn);
            }
        }

        this.pendingCommands.Dispose();
        await this.Connection.DisposeAsync().ConfigureAwait(false);
        this.connectDisconnectSemaphore.Dispose();
    }

    /// <summary>
    /// Asynchronously acquires the connection lock to ensure thread-safe operations for connection-related actions.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous acquire operation.</returns>
    protected virtual async Task AcquireConnectionLockAsync(CancellationToken cancellationToken = default)
    {
        await this.connectDisconnectSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Releases the connection lock to allow other threads to perform connection-related actions.
    /// </summary>
    protected virtual void ReleaseConnectionLock()
    {
        this.connectDisconnectSemaphore.Release();
    }

    [ExcludeFromCodeCoverage]
    private static IJsonTypeInfoResolver CreateTypeInfoResolver()
    {
        return JsonSerializer.IsReflectionEnabledByDefault
            ? new DefaultJsonTypeInfoResolver()
            : WebDriverBiDiJsonSerializerContext.Default;
    }

    private static string TruncateMessage(string message, int maxLength)
    {
        if (message.Length <= maxLength)
        {
            return message;
        }

        return $"{message.Substring(0, maxLength)}...";
    }

    private async Task OnProtocolEventReceivedAsync(EventReceivedEventArgs e)
    {
        try
        {
            await this.OnEventReceived.NotifyObserversAsync(e);
        }
        catch (Exception ex)
        {
            WebDriverBiDiEventSource.RaiseEvent.EventHandlerError(e.EventName, ex.Message);
            this.CaptureUnhandledError(UnhandledErrorType.EventHandlerException, ex, $"Unhandled exception in user event handler for event name {e.EventName}");
        }
    }

    private async Task OnProtocolErrorEventReceivedAsync(ErrorReceivedEventArgs e)
    {
        try
        {
            await this.OnErrorEventReceived.NotifyObserversAsync(e);
        }
        catch (Exception ex)
        {
            this.CaptureUnhandledError(UnhandledErrorType.EventHandlerException, ex, $"Unhandled exception in user event handler for error event");
        }
    }

    private async Task OnProtocolUnknownMessageReceivedAsync(UnknownMessageReceivedEventArgs e)
    {
        try
        {
            await this.OnUnknownMessageReceived.NotifyObserversAsync(e);
        }
        catch (Exception ex)
        {
            this.CaptureUnhandledError(UnhandledErrorType.EventHandlerException, ex, $"Unhandled exception in user event handler for unknown message event");
        }
    }

    private async Task OnConnectionDataReceivedAsync(ConnectionDataReceivedEventArgs e)
    {
        await this.queue.Writer.WriteAsync(e.Data);
    }

    private async Task OnConnectionErrorAsync(ConnectionErrorEventArgs e)
    {
        WebDriverBiDiEventSource.RaiseEvent.ConnectionError(this.Connection.GetHashCode().ToString(), e.Exception.Message);

        await this.AcquireConnectionLockAsync(CancellationToken.None).ConfigureAwait(false);
        try
        {
            // Only process if we were connected (or thought we were).
            // If we're still in ConnectAsync, we'll run after it releases the lock,
            // so we'll see the correct post-connect state.
            if (!this.IsConnected)
            {
                // ConnectAsync hasn't set it yet, or we're already disconnected
                return;
            }

            // Mark the transport as disconnected so that subsequent SendCommandAsync
            // calls fail immediately rather than queuing commands that can never
            // receive a response.
            this.IsConnected = false;

            // Close the pending command collection and fail every in-flight command
            // with an exception that wraps the original connection error.
            await this.pendingCommands.CloseAsync().ConfigureAwait(false);
            WebDriverBiDiConnectionException connectionException = new($"Unexpected connection error: {e.Exception.Message}", e.Exception);
            this.pendingCommands.FailAllPendingCommands(connectionException);

            await this.LogAsync($"Connection error; pending commands failed: {e.Exception.Message}", WebDriverBiDiLogLevel.Error);
        }
        finally
        {
            this.ReleaseConnectionLock();
        }
    }

    private async Task ReadIncomingMessagesAsync()
    {
        // In theory, we could accomplish this with an `await foreach` using
        // IAsyncEnumerable, but this would require additional dependencies,
        // which is challenging. Initial experiments has shown that simply
        // adding a reference to the Microsoft.Bcl.AsyncInterfaces assembly
        // is not enough by itself to enable compilation using that construct.
        while (await this.queue.Reader.WaitToReadAsync().ConfigureAwait(false))
        {
            while (this.queue.Reader.TryRead(out byte[]? incomingMessage))
            {
                try
                {
                    await this.ProcessMessageAsync(incomingMessage);
                }
                catch (Exception ex)
                {
                    await this.LogAsync($"Unexpected error in message processing loop: {ex.Message}", WebDriverBiDiLogLevel.Error);
                    this.CaptureUnhandledError(UnhandledErrorType.ProtocolError, ex, "Unexpected error in message processing loop");
                }
            }
        }
    }

    private async Task ProcessMessageAsync(byte[] messageData)
    {
        bool isProcessed = false;
        JsonElement messageRootElement = default;
        try
        {
            messageRootElement = this.DeserializeMessage(messageData);
        }
        catch (JsonException e)
        {
            // JSON parsing errors are regarded as "unknown message" errors, rather than
            // "protocol errors." Protocol errors are defined as valid JSON messages that
            // resemble protocol data structures, but do not fit the payload definitions
            // of the protocol. Here, we just log the error; the error will be processed
            // to be added to the proper unhandled error collection in the "if (!isProcessed)"
            // block below.
            await this.LogAsync($"Unexpected error parsing JSON message: {e.Message}", WebDriverBiDiLogLevel.Error);
        }

        if (messageRootElement.ValueKind != JsonValueKind.Undefined)
        {
            if (messageRootElement.TryGetProperty("type", out JsonElement messageTypeToken) && messageTypeToken.ValueKind == JsonValueKind.String)
            {
                string messageType = messageTypeToken.GetString()!;
                bool isLogging = this.OnLogMessage.CurrentObserverCount > 0;
                string loggingMessageData = string.Empty;
                if (isLogging)
                {
                    loggingMessageData = Encoding.UTF8.GetString(messageData);
                }

                if (messageType == "success")
                {
                    isProcessed = this.ProcessCommandResponseMessage(messageRootElement);
                    if (isLogging)
                    {
                        await this.LogAsync($"Command response message processed {loggingMessageData}", WebDriverBiDiLogLevel.Trace);
                    }
                }
                else if (messageType == "error")
                {
                    isProcessed = await this.ProcessErrorMessageAsync(messageRootElement);
                    if (isLogging)
                    {
                        await this.LogAsync($"Error response message processed {loggingMessageData}", WebDriverBiDiLogLevel.Trace);
                    }
                }
                else if (messageType == "event")
                {
                    isProcessed = await this.ProcessEventMessageAsync(messageRootElement);
                    if (isLogging)
                    {
                        await this.LogAsync($"Event message processed {loggingMessageData}", WebDriverBiDiLogLevel.Trace);
                    }
                }
            }
        }

        if (!isProcessed)
        {
            string message = Encoding.UTF8.GetString(messageData);
            string messageType = messageRootElement.ValueKind != JsonValueKind.Undefined && messageRootElement.TryGetProperty("type", out JsonElement typeElement)
                ? typeElement.GetString() ?? "unknown"
                : "unknown";
            WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived(messageType, messageData.Length);
            await this.OnProtocolUnknownMessageReceivedAsync(new UnknownMessageReceivedEventArgs(message));
            this.CaptureUnhandledError(UnhandledErrorType.UnknownMessage, new WebDriverBiDiException($"Received unknown message from protocol connection: {message}"), "Unknown message from connection");
        }
    }

    private bool ProcessCommandResponseMessage(JsonElement message)
    {
        if (message.TryGetProperty("id", out JsonElement idToken) && idToken.ValueKind == JsonValueKind.Number && idToken.TryGetInt64(out long responseId))
        {
            if (this.pendingCommands.RemovePendingCommand(responseId, out Command? executedCommand))
            {
                // Stop timing and log completion
                if (this.commandTimings.TryRemove(responseId, out Stopwatch? stopwatch))
                {
                    stopwatch.Stop();
                    WebDriverBiDiEventSource.RaiseEvent.CommandCompleted(responseId.ToString(), executedCommand.CommandName, stopwatch.ElapsedMilliseconds);
                }

                try
                {
                    if (message.Deserialize(executedCommand.ResponseType, this.options) is CommandResponseMessage response)
                    {
                        CommandResult commandResult = response.Result;
                        commandResult.AdditionalData = response.AdditionalData;
                        executedCommand.SetResult(commandResult);
                    }
                }
                catch (Exception ex)
                {
                    executedCommand.SetException(new WebDriverBiDiSerializationException($"Response did not contain properly formed JSON for response type (response JSON:{message})", ex));
                }

                return true;
            }
        }

        return false;
    }

    private async Task<bool> ProcessErrorMessageAsync(JsonElement message)
    {
        try
        {
            // If the message doesn't match the schema of an actual error message,
            // an exception will be thrown by the JSON serializer, and we can log
            // the malformed response.
            ErrorResponseMessage? errorMessage = message.Deserialize<ErrorResponseMessage>(this.options);
            if (errorMessage is not null)
            {
                ErrorResult result = errorMessage.GetErrorResponseData();
                if (errorMessage.CommandId.HasValue && this.pendingCommands.RemovePendingCommand(errorMessage.CommandId.Value, out Command? executedCommand))
                {
                    // Stop timing and log error
                    if (this.commandTimings.TryRemove(errorMessage.CommandId.Value, out Stopwatch? stopwatch))
                    {
                        stopwatch.Stop();
                    }

                    WebDriverBiDiEventSource.RaiseEvent.CommandError(errorMessage.CommandId.Value.ToString(), executedCommand.CommandName, result.ErrorType, result.ErrorMessage);

                    executedCommand.SetResult(result);
                }
                else
                {
                    await this.OnProtocolErrorEventReceivedAsync(new ErrorReceivedEventArgs(result));
                    this.CaptureUnhandledError(UnhandledErrorType.UnexpectedError, new WebDriverBiDiProtocolException($"Received '{result.ErrorType}' error with no command ID: {result.ErrorMessage}", result), "Received error with no command ID");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            await this.LogAsync($"Unexpected error parsing error JSON: {ex.Message} (JSON: {message})", WebDriverBiDiLogLevel.Error);
            this.CaptureUnhandledError(UnhandledErrorType.ProtocolError, ex, $"Invalid JSON in protocol error response: {message}");
            WebDriverBiDiEventSource.RaiseEvent.ProtocolError(ex.Message, TruncateMessage(message.ToString(), 100));
        }

        return false;
    }

    private async Task<bool> ProcessEventMessageAsync(JsonElement message)
    {
        if (message.TryGetProperty("method", out JsonElement eventNameToken) && eventNameToken.ValueKind == JsonValueKind.String)
        {
            // We have already validated that the token is of type string,
            // and therefore will never be null.
            string eventName = eventNameToken.GetString()!;
            if (this.eventMessageTypes.TryGetValue(eventName, out Type? eventMessageType))
            {
                try
                {
                    if (message.Deserialize(eventMessageType, this.options) is not EventMessage eventMessageData)
                    {
                        throw new WebDriverBiDiSerializationException($"Deserialization of event message returned null for event type {eventMessageType}");
                    }

                    WebDriverBiDiEventSource.RaiseEvent.EventReceived(eventName);
                    await this.OnProtocolEventReceivedAsync(new EventReceivedEventArgs(eventMessageData));
                    return true;
                }
                catch (Exception ex)
                {
                    await this.LogAsync($"Unexpected error parsing event JSON: {ex.Message} (JSON: {message})", WebDriverBiDiLogLevel.Error);
                    this.CaptureUnhandledError(UnhandledErrorType.ProtocolError, ex, $"Invalid JSON in event message: {message}");
                    WebDriverBiDiEventSource.RaiseEvent.ProtocolError(ex.Message, TruncateMessage(message.ToString(), 100));
                }
            }
        }

        return false;
    }

    private void CaptureUnhandledError(UnhandledErrorType errorType, Exception ex, string terminalReason)
    {
        bool isTerminalError = false;
        switch (errorType)
        {
            case UnhandledErrorType.ProtocolError:
                isTerminalError = this.ProtocolErrorBehavior == TransportErrorBehavior.Terminate;
                break;
            case UnhandledErrorType.UnknownMessage:
                isTerminalError = this.UnknownMessageBehavior == TransportErrorBehavior.Terminate;
                break;
            case UnhandledErrorType.UnexpectedError:
                isTerminalError = this.UnexpectedErrorBehavior == TransportErrorBehavior.Terminate;
                break;
            case UnhandledErrorType.EventHandlerException:
                isTerminalError = this.EventHandlerExceptionBehavior == TransportErrorBehavior.Terminate;
                break;
        }

        // Note carefully that if handling of the error type is "Ignore", adding to the
        // unhandled errors collection is a no-op.
        this.unhandledErrors.AddUnhandledError(errorType, ex);
        if (isTerminalError)
        {
            this.terminationReason = terminalReason;
        }
    }

    private async Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.OnLogMessage.NotifyObserversAsync(new LogMessageEventArgs(message, level, "Transport"));
    }

    private async Task OnConnectionLogMessageAsync(LogMessageEventArgs e)
    {
        await this.OnLogMessage.NotifyObserversAsync(e);
    }

    private Exception CreateTerminationException(IList<Exception> exceptions, TransportErrorBehavior errorBehavior = TransportErrorBehavior.Terminate)
    {
        string message = $"Unhandled exception during transport operations. Transport was terminated with the following reason: {this.terminationReason}";
        if (errorBehavior != TransportErrorBehavior.Collect && exceptions.Count == 1)
        {
            return new WebDriverBiDiException(message, exceptions[0]);
        }

        return new AggregateException(message, exceptions);
    }
}
