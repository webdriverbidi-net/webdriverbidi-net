// <copyright file="Transport.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The transport object used for serializing and deserializing JSON data used in the WebDriver Bidi protocol.
/// It uses a <see cref="Connection"/> object to communicate with the remote end, and does no further processing
/// of the objects serialized or deserialized. Consumers of this class are expected to handle things like awaiting
/// the response of a WebDriver BiDi command message.
/// </summary>
public class Transport
{
    private const string EventReceivedEventName = "transport.eventReceived";
    private const string ErrorReceivedEventName = "transport.errorReceived";
    private const string UnknownMessageReceivedEventName = "transport.unknownMessageReceived";
    private const string LogMessageEventName = "transport.logMessage";

    private readonly JsonSerializerOptions options = new()
    {
        TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault
            ? new PrivateConstructorContractResolver()
            : WebDriverBiDiJsonSerializerContext.Default,
    };

    private readonly Dictionary<string, Type> eventMessageTypes = [];
    private readonly UnhandledErrorCollection unhandledErrors = new();
    private Channel<byte[]> queue = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions()
    {
        SingleReader = true,
        SingleWriter = true,
    });

    private PendingCommandCollection pendingCommands = new();
    private Task messageQueueProcessingTask = Task.CompletedTask;
    private long nextCommandId = 0;
    private bool isConnected;
    private string terminationReason = "Normal shutdown";

    /// <summary>
    /// Initializes a new instance of the <see cref="Transport"/> class.
    /// </summary>
    public Transport()
        : this(new Connection())
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
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task ConnectAsync(string websocketUri)
    {
        if (this.isConnected)
        {
            throw new WebDriverBiDiException($"The transport is already connected to {this.Connection.ConnectedUrl}; you must disconnect before connecting to another URL");
        }

        if (!this.pendingCommands.IsAcceptingCommands)
        {
            this.pendingCommands = new PendingCommandCollection();
        }

        this.queue = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true,
        });

        this.unhandledErrors.ClearUnhandledErrors();

        // Reset the command counter for each connection.
        this.nextCommandId = 0;
        this.messageQueueProcessingTask = Task.Run(() => this.ReadIncomingMessages());
        if (!this.Connection.IsActive)
        {
            // Allow for the possibility of the connection to already being opened.
            await this.Connection.StartAsync(websocketUri).ConfigureAwait(false);
        }

        this.isConnected = true;
    }

    /// <summary>
    /// Asynchronously disconnects from the remote end web socket.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task DisconnectAsync()
    {
        await this.DisconnectAsync(true);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end.
    /// </summary>
    /// <param name="commandData">The command settings object containing all data required to execute the command.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the command ID is already in use.</exception>
    public virtual async Task<Command> SendCommandAsync(CommandParameters commandData)
    {
        if (this.unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Terminate))
        {
            await this.DisconnectAsync(false).ConfigureAwait(false);
            throw this.CreateTerminationException();
        }

        if (!this.isConnected)
        {
            throw new WebDriverBiDiException("Transport must be connected to a remote end to execute commands.");
        }

        long commandId = this.GetNextCommandId();
        Command command = new(commandId, commandData);
        this.pendingCommands.AddPendingCommand(command);
        byte[] commandJson = this.SerializeCommand(command);
        await this.Connection.SendDataAsync(commandJson).ConfigureAwait(false);
        return command;
    }

    /// <summary>
    /// Registers an event message to be recognized when received from the connection.
    /// </summary>
    /// <typeparam name="T">The type of data to be returned in the event.</typeparam>
    /// <param name="eventName">The name of the event.</param>
    public virtual void RegisterEventMessage<T>(string eventName)
    {
        this.eventMessageTypes[eventName] = typeof(EventMessage<T>);
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
        try
        {
            JsonDocument document = JsonDocument.Parse(messageData);
            return document.RootElement;
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Asynchronously disconnects from the remote end web socket.
    /// </summary>
    /// <param name="throwCollectedExceptions"> A value indicating whether to throw the collected exceptions.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual async Task DisconnectAsync(bool throwCollectedExceptions)
    {
        // Close the pending command collection to further addition of commands,
        // and stop the connection from receiving further communication traffic.
        this.pendingCommands.Close();
        await this.Connection.StopAsync().ConfigureAwait(false);

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
        // the message queue.
        // CONSIDER: This may introduce a hang during shutdown, if an in-process
        // event handler hangs. It might be worthwhile to introduce a timeout
        // for this use case if it becomes a problem reported by users.
        this.pendingCommands.Clear();
        await this.messageQueueProcessingTask;

        // Finally, mark the transport as disconnected, and throw collected
        // exceptions if the user has specified that behavior.
        this.isConnected = false;
        if (throwCollectedExceptions && this.unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect))
        {
            throw this.CreateTerminationException();
        }
    }

    private async Task OnProtocolEventReceivedAsync(EventReceivedEventArgs e)
    {
        try
        {
            await this.OnEventReceived.NotifyObserversAsync(e);
        }
        catch (Exception ex)
        {
            this.CaptureUnhandledError(UnhandledErrorType.EventHandlerException, ex, $"Unhandled exception in user event handler for event name {e.EventName}");
        }
    }

    private async Task OnProtocolErrorEventReceivedAsync(ErrorReceivedEventArgs e)
    {
        await this.OnErrorEventReceived.NotifyObserversAsync(e);
    }

    private async Task OnProtocolUnknownMessageReceivedAsync(UnknownMessageReceivedEventArgs e)
    {
        await this.OnUnknownMessageReceived.NotifyObserversAsync(e);
    }

    private async Task OnConnectionDataReceivedAsync(ConnectionDataReceivedEventArgs e)
    {
        await this.queue.Writer.WriteAsync(e.Data);
    }

    private async Task ReadIncomingMessages()
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
                await this.ProcessMessageAsync(incomingMessage);
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
            await this.LogAsync($"Unexpected error parsing JSON message: {e.Message}", WebDriverBiDiLogLevel.Error);
            this.CaptureUnhandledError(UnhandledErrorType.ProtocolError, e, $"Invalid JSON in protocol message: {Encoding.UTF8.GetString(messageData)}");
        }

        if (messageRootElement.ValueKind != JsonValueKind.Undefined)
        {
            if (messageRootElement.TryGetProperty("type", out JsonElement messageTypeToken) && messageTypeToken.ValueKind == JsonValueKind.String)
            {
                string messageType = messageTypeToken.GetString()!;
                if (messageType == "success")
                {
                    isProcessed = this.ProcessCommandResponseMessage(messageRootElement);

                    if (this.OnLogMessage.CurrentObserverCount > 0)
                    {
                        await this.LogAsync($"Command response message processed {Encoding.UTF8.GetString(messageData)}", WebDriverBiDiLogLevel.Trace);
                    }
                }
                else if (messageType == "error")
                {
                    isProcessed = await this.ProcessErrorMessageAsync(messageRootElement);

                    if (this.OnLogMessage.CurrentObserverCount > 0)
                    {
                        await this.LogAsync($"Error response message processed {Encoding.UTF8.GetString(messageData)}", WebDriverBiDiLogLevel.Trace);
                    }
                }
                else if (messageType == "event")
                {
                    isProcessed = await this.ProcessEventMessageAsync(messageRootElement);

                    if (this.OnLogMessage.CurrentObserverCount > 0)
                    {
                        await this.LogAsync($"Event message processed {Encoding.UTF8.GetString(messageData)}", WebDriverBiDiLogLevel.Trace);
                    }
                }
            }
        }

        if (!isProcessed)
        {
            string message = Encoding.UTF8.GetString(messageData);
            await this.OnProtocolUnknownMessageReceivedAsync(new UnknownMessageReceivedEventArgs(message));
            this.CaptureUnhandledError(UnhandledErrorType.UnknownMessage, new WebDriverBiDiException($"Received unknown message from protocol connection: {message}"), "Unknown message from connection");
        }
    }

    private bool ProcessCommandResponseMessage(JsonElement message)
    {
        if (message.TryGetProperty("id", out JsonElement idToken) && idToken.ValueKind == JsonValueKind.Number && idToken.TryGetInt64(out long responseId))
        {
            if (this.pendingCommands.RemovePendingCommand(responseId, out Command executedCommand))
            {
                try
                {
                    if (message.Deserialize(executedCommand.ResponseType, this.options) is CommandResponseMessage response)
                    {
                        CommandResult commandResult = response.Result;
                        commandResult.AdditionalData = response.AdditionalData;
                        executedCommand.Result = commandResult;
                    }
                }
                catch (Exception ex)
                {
                    executedCommand.ThrownException = new WebDriverBiDiException($"Response did not contain properly formed JSON for response type (response JSON:{message})", ex);
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
                if (errorMessage.CommandId.HasValue && this.pendingCommands.RemovePendingCommand(errorMessage.CommandId.Value, out Command executedCommand))
                {
                    executedCommand.Result = result;
                }
                else
                {
                    await this.OnProtocolErrorEventReceivedAsync(new ErrorReceivedEventArgs(result));
                    this.CaptureUnhandledError(UnhandledErrorType.UnexpectedError, new WebDriverBiDiException($"Received '{result.ErrorType}' error with no command ID: {result.ErrorMessage}"), "Received error with no command ID");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            await this.LogAsync($"Unexpected error parsing error JSON: {ex.Message} (JSON: {message})", WebDriverBiDiLogLevel.Error);
            this.CaptureUnhandledError(UnhandledErrorType.ProtocolError, ex, $"Invalid JSON in protocol error response: {message}");
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
                    // Deserialize will correctly throw if the type does not match, meaning
                    // the eventMessageData variable can never be null.
                    EventMessage? eventMessageData = message.Deserialize(eventMessageType, this.options) as EventMessage;
                    await this.OnProtocolEventReceivedAsync(new EventReceivedEventArgs(eventMessageData!));
                    return true;
                }
                catch (Exception ex)
                {
                    await this.LogAsync($"Unexpected error parsing event JSON: {ex.Message} (JSON: {message})", WebDriverBiDiLogLevel.Error);
                    this.CaptureUnhandledError(UnhandledErrorType.ProtocolError, ex, $"Invalid JSON in event message: {message}");
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

    private Exception CreateTerminationException()
    {
        string message = $"Unhandled exception during transport operations. Transport was terminated with the following reason: {this.terminationReason}";
        if (this.unhandledErrors.Exceptions.Count == 1)
        {
            return new WebDriverBiDiException(message, this.unhandledErrors.Exceptions[0]);
        }

        return new AggregateException(message, this.unhandledErrors.Exceptions);
    }
}
