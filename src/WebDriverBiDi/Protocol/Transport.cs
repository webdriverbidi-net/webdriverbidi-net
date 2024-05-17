// <copyright file="Transport.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The transport object used for serializing and deserializing JSON data used in the WebDriver Bidi protocol.
/// It uses a <see cref="Connection"/> object to communicate with the remote end, and does no further processing
/// of the objects serialized or deserialized. Consumers of this class are expected to handle things like awaiting
/// the response of a WebDriver BiDi command message.
/// </summary>
public class Transport
{
    private readonly JsonSerializerOptions options = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    private readonly Connection connection;
    private readonly Dictionary<string, Type> eventMessageTypes = new();
    private readonly UnhandledErrorCollection unhandledErrors = new();
    private PendingCommandCollection pendingCommands = new();
    private Dispatcher<string> incomingMessageQueue = new();
    private Dispatcher<EventMessage> eventDispatcher = new();
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
        this.connection = connection;
        this.incomingMessageQueue.ItemDispatched += this.OnIncomingMessageDispatched;
        this.eventDispatcher.ItemDispatched += this.OnEventDispatched;
        connection.DataReceived += this.OnConnectionDataReceived;
        connection.LogMessage += this.OnConnectionLogMessage;
    }

    /// <summary>
    /// Occurs when a message is logged.
    /// </summary>
    public event EventHandler<LogMessageEventArgs>? LogMessage;

    /// <summary>
    /// Occurs when an event is received from the protocol.
    /// </summary>
    public event EventHandler<EventReceivedEventArgs>? EventReceived;

    /// <summary>
    /// Occurs when an error is received from the protocol that is not the result of a command execution.
    /// </summary>
    public event EventHandler<ErrorReceivedEventArgs>? ErrorEventReceived;

    /// <summary>
    /// Occurs when an unknown message is received from the protocol.
    /// </summary>
    public event EventHandler<UnknownMessageReceivedEventArgs>? UnknownMessageReceived;

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
    /// Asynchronously connects to the remote end web socket.
    /// </summary>
    /// <param name="websocketUri">The URI used to connect to the web socket.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task ConnectAsync(string websocketUri)
    {
        if (this.isConnected)
        {
            throw new WebDriverBiDiException($"The transport is already connected to {this.connection.ConnectedUrl}; you must disconnect before connecting to another URL");
        }

        if (!this.pendingCommands.IsAcceptingCommands)
        {
            this.pendingCommands = new PendingCommandCollection();
        }

        if (!this.incomingMessageQueue.IsDispatching)
        {
            this.incomingMessageQueue = new Dispatcher<string>();
            this.incomingMessageQueue.ItemDispatched += this.OnIncomingMessageDispatched;
        }

        if (!this.eventDispatcher.IsDispatching)
        {
            this.eventDispatcher = new Dispatcher<EventMessage>();
            this.eventDispatcher.ItemDispatched += this.OnEventDispatched;
        }

        this.unhandledErrors.ClearUnhandledErrors();

        // Reset the command counter for each connection.
        this.nextCommandId = 0;
        await this.connection.StartAsync(websocketUri).ConfigureAwait(false);
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

        long commandId = Interlocked.Increment(ref this.nextCommandId);
        Command command = new(commandId, commandData);
        this.pendingCommands.AddPendingCommand(command);
        string commandJson = JsonSerializer.Serialize(command);
        await this.connection.SendDataAsync(commandJson).ConfigureAwait(false);
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
    /// Asynchronously disconnects from the remote end web socket.
    /// </summary>
    /// <param name="throwCollectedExceptions"> A value indicating whether to throw the collected exceptions.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual async Task DisconnectAsync(bool throwCollectedExceptions)
    {
        // Steps in the disconnect process:
        // 1. Close the pending command collection to further addition of commands.
        // 2. Stop the connection from receiving further communication traffic.
        // 3. Stop dispatching incoming command responses. Note that the dispatcher
        //    will attempt to dispatch any pending responses already in the queue.
        //    Dispatching the pending responses in the queue and processing those
        //    responses  will also remove those commands from the pending command
        //    collection.
        // 4. Stop dispatching incoming event messages. Note that the dispatcher will
        //    attempt to dispatch any pending event messages already in the queue.
        // 5. Clear the pending command collection. This will also cancel any tasks
        //    associated with the remaining pending commands.
        this.pendingCommands.Close();
        await this.connection.StopAsync().ConfigureAwait(false);
        this.incomingMessageQueue.StopDispatching();
        this.eventDispatcher.StopDispatching();
        this.pendingCommands.Clear();
        this.isConnected = false;
        if (throwCollectedExceptions && this.unhandledErrors.HasUnhandledErrors(TransportErrorBehavior.Collect))
        {
            throw this.CreateTerminationException();
        }
    }

    /// <summary>
    /// Raises the LogMessage event.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The EventArgs containing information about the event.</param>
    protected virtual void OnLogMessage(object? sender, LogMessageEventArgs e)
    {
        if (this.LogMessage is not null)
        {
            this.LogMessage(this, e);
        }
    }

    /// <summary>
    /// Raises the EventReceived event.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The EventArgs containing information about the event.</param>
    protected virtual void OnProtocolEventReceived(object? sender, EventReceivedEventArgs e)
    {
        try
        {
            if (this.EventReceived is not null)
            {
                this.EventReceived(this, e);
            }
        }
        catch (Exception ex)
        {
            this.CaptureUnhandledError(UnhandledErrorType.EventHandlerException, ex, $"Unhandled exception in user event handler for event name {e.EventName}");
        }
    }

    /// <summary>
    /// Raises the ErrorEventReceived event.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The EventArgs containing information about the event.</param>
    protected virtual void OnProtocolErrorEventReceived(object? sender, ErrorReceivedEventArgs e)
    {
        if (this.ErrorEventReceived is not null)
        {
            this.ErrorEventReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the UnknownMessageReceived event.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The EventArgs containing information about the event.</param>
    protected virtual void OnProtocolUnknownMessageReceived(object? sender, UnknownMessageReceivedEventArgs e)
    {
        if (this.UnknownMessageReceived is not null)
        {
            this.UnknownMessageReceived(this, e);
        }
    }

    private void OnConnectionDataReceived(object? sender, ConnectionDataReceivedEventArgs e)
    {
        this.incomingMessageQueue.TryDispatch(e.Data);
    }

    private void OnIncomingMessageDispatched(object sender, ItemDispatchedEventArgs<string> e)
    {
        this.ProcessMessage(e.DispatchedItem);
    }

    private void ProcessMessage(string messageData)
    {
        bool isProcessed = false;
        JsonDocument? message = null;
        try
        {
            message = JsonDocument.Parse(messageData);
        }
        catch (JsonException e)
        {
            this.Log($"Unexpected error parsing JSON message: {e.Message}", WebDriverBiDiLogLevel.Error);
            this.CaptureUnhandledError(UnhandledErrorType.ProtocolError, e, $"Invalid JSON in protocol message: {messageData}");
        }

        if (message is not null)
        {
            if (message.RootElement.TryGetProperty("type", out JsonElement messageTypeToken) && messageTypeToken.ValueKind == JsonValueKind.String)
            {
                string messageType = messageTypeToken.GetString()!;
                if (messageType == "success")
                {
                    isProcessed = this.ProcessCommandResponseMessage(message.RootElement);
                    this.Log($"Command response message processed {message}", WebDriverBiDiLogLevel.Trace);
                }
                else if (messageType == "error")
                {
                    isProcessed = this.ProcessErrorMessage(message.RootElement);
                    this.Log($"Error response message processed {message}", WebDriverBiDiLogLevel.Trace);
                }
                else if (messageType == "event")
                {
                    isProcessed = this.ProcessEventMessage(message.RootElement);
                    this.Log($"Event message processed {message}", WebDriverBiDiLogLevel.Trace);
                }
            }
        }

        if (!isProcessed)
        {
            this.OnProtocolUnknownMessageReceived(this, new UnknownMessageReceivedEventArgs(messageData));
            this.CaptureUnhandledError(UnhandledErrorType.UnknownMessage, new WebDriverBiDiException($"Received unknown message from protocol connection: {messageData}"), "Unknown message from connection");
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

    private bool ProcessErrorMessage(JsonElement message)
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
                    this.OnProtocolErrorEventReceived(this, new ErrorReceivedEventArgs(result));
                    this.CaptureUnhandledError(UnhandledErrorType.UnexpectedError, new WebDriverBiDiException($"Received '{result.ErrorType}' error with no command ID: {result.ErrorMessage}"), "Received error with no command ID");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            this.Log($"Unexpected error parsing error JSON: {ex.Message} (JSON: {message})", WebDriverBiDiLogLevel.Error);
            this.CaptureUnhandledError(UnhandledErrorType.ProtocolError, ex, $"Invalid JSON in protocol error response: {message}");
        }

        return false;
    }

    private bool ProcessEventMessage(JsonElement message)
    {
        if (message.TryGetProperty("method", out JsonElement eventNameToken) && eventNameToken.ValueKind == JsonValueKind.String)
        {
            // We have already validated that the token is of type string,
            // and therefore will never be null.
            string eventName = eventNameToken.GetString()!;
            if (this.eventMessageTypes.TryGetValue(eventName, out Type eventMessageType))
            {
                try
                {
                    // Deserialize will correctly throw if the type does not match, meaning
                    // the eventMessageData variable can never be null.
                    EventMessage? eventMessageData = message.Deserialize(eventMessageType, this.options) as EventMessage;
                    this.eventDispatcher.TryDispatch(eventMessageData!);
                    return true;
                }
                catch (Exception ex)
                {
                    this.Log($"Unexpected error parsing event JSON: {ex.Message} (JSON: {message})", WebDriverBiDiLogLevel.Error);
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

    private void OnEventDispatched(object sender, ItemDispatchedEventArgs<EventMessage> e)
    {
        this.OnProtocolEventReceived(this, new EventReceivedEventArgs(e.DispatchedItem));
    }

    private void Log(string message, WebDriverBiDiLogLevel level)
    {
        this.OnLogMessage(this, new LogMessageEventArgs(message, level, "Transport"));
    }

    private void OnConnectionLogMessage(object? sender, LogMessageEventArgs e)
    {
        this.OnLogMessage(sender, e);
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
