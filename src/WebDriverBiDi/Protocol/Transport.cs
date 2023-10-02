// <copyright file="Transport.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The transport object used for serializing and deserializing JSON data used in the WebDriver Bidi protocol.
/// </summary>
public class Transport
{
    private readonly JsonSerializerOptions options = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    private readonly ConcurrentDictionary<long, Command> pendingCommands = new();
    private readonly Connection connection;
    private readonly TimeSpan commandWaitTimeout;
    private readonly Dictionary<string, Type> eventMessageTypes = new();
    private Dispatcher<string> incomingMessageQueue = new();
    private Dispatcher<EventMessage> eventDispatcher = new();
    private long nextCommandId = 0;
    private bool isConnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="Transport"/> class.
    /// </summary>
    public Transport()
        : this(Timeout.InfiniteTimeSpan)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Transport"/> class with a given command timeout.
    /// </summary>
    /// <param name="commandWaitTimeout">The timeout used to wait for execution of commands.</param>
    public Transport(TimeSpan commandWaitTimeout)
        : this(commandWaitTimeout, new Connection())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Transport"/> class with a given command timeout and connection.
    /// </summary>
    /// <param name="commandWaitTimeout">The timeout used to wait for execution of commands.</param>
    /// <param name="connection">The connection used to communicate with the protocol remote end.</param>
    public Transport(TimeSpan commandWaitTimeout, Connection connection)
    {
        this.commandWaitTimeout = commandWaitTimeout;
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
    /// Occurs when an error is received from the protocol.
    /// </summary>
    public event EventHandler<ErrorReceivedEventArgs>? ErrorEventReceived;

    /// <summary>
    /// Occurs when an unknown message is received from the protocol.
    /// </summary>
    public event EventHandler<UnknownMessageReceivedEventArgs>? UnknownMessageReceived;

    /// <summary>
    /// Gets the ID of the last command to be added.
    /// </summary>
    protected long LastCommandId => this.nextCommandId;

    /// <summary>
    /// Asynchronously connects to the remote end web socket.
    /// </summary>
    /// <param name="websocketUri">The URI used to connect to the web socket.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task ConnectAsync(string websocketUri)
    {
        if (this.isConnected)
        {
            throw new WebDriverBiDiException($"The transport is already connected to {this.connection.ConnectedUrl}; you must disconnect before connecting to another URL");
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

        // Reset the command counter for each connection.
        this.nextCommandId = 0;
        await this.connection.StartAsync(websocketUri).ConfigureAwait(false);
        this.isConnected = true;
    }

    /// <summary>
    /// Asynchronously disconnects from the remote end web socket.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task DisconnectAsync()
    {
        await this.connection.StopAsync().ConfigureAwait(false);
        this.incomingMessageQueue.StopDispatching();
        this.eventDispatcher.StopDispatching();
        this.pendingCommands.Clear();
        this.isConnected = false;
    }

    /// <summary>
    /// Asynchronously send a command to the remote end and waits for a response.
    /// </summary>
    /// <param name="command">The command settings object containing all data required to execute the command.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task<CommandResult> SendCommandAndWaitAsync(CommandParameters command)
    {
        long commandId = await this.SendCommandAsync(command).ConfigureAwait(false);
        await this.WaitForCommandCompleteAsync(commandId, this.commandWaitTimeout);
        return this.GetCommandResponse(commandId);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end.
    /// </summary>
    /// <param name="command">The command settings object containing all data required to execute the command.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the command ID is already in use.</exception>
    public virtual async Task<long> SendCommandAsync(CommandParameters command)
    {
        long commandId = Interlocked.Increment(ref this.nextCommandId);
        Command executionData = new(commandId, command);
        if (!this.AddPendingCommand(executionData))
        {
            throw new WebDriverBiDiException($"Could not add command with id {executionData.CommandId}, as id already exists");
        }

        string commandJson = JsonSerializer.Serialize(executionData);
        await this.connection.SendDataAsync(commandJson).ConfigureAwait(false);
        return executionData.CommandId;
    }

    /// <summary>
    /// Asynchronously waits for a command with the given ID to complete.
    /// </summary>
    /// <param name="commandId">The ID of the command for which to wait for completion.</param>
    /// <param name="waitTimeout">The timeout describing how long to wait for the command to complete.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the command ID is invalid or if the command times out.</exception>
    public virtual async Task WaitForCommandCompleteAsync(long commandId, TimeSpan waitTimeout)
    {
        if (!this.pendingCommands.ContainsKey(commandId))
        {
            throw new WebDriverBiDiException($"Unknown command id {commandId}");
        }
        else
        {
            if (!await this.pendingCommands[commandId].WaitForCompletionAsync(waitTimeout).ConfigureAwait(false))
            {
                throw new WebDriverBiDiException($"Timed out waiting for response for command id {commandId}");
            }
        }
    }

    /// <summary>
    /// Gets the result of the command.
    /// </summary>
    /// <param name="commandId">The ID of the command for which to get the response.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the command result could not be retrieved, or if the command result is not valid.</exception>
    public virtual CommandResult GetCommandResponse(long commandId)
    {
        if (this.pendingCommands.TryRemove(commandId, out Command? command))
        {
            if (command.Result is null)
            {
                if (command.ThrownException is null)
                {
                    throw new WebDriverBiDiException($"Result and thrown exception for command with id {commandId} are both null");
                }

                throw command.ThrownException;
            }

            return command.Result;
        }

        throw new WebDriverBiDiException($"Could not remove command with id {commandId}");
    }

    /// <summary>
    /// Registers an event message to be recognized when received from the connection.
    /// </summary>
    /// <typeparam name="T">The type of data to be returned in the event.</typeparam>
    /// <param name="eventName">The name of the event.</param>
    public void RegisterEventMessage<T>(string eventName)
    {
        this.eventMessageTypes[eventName] = typeof(EventMessage<T>);
    }

    /// <summary>
    /// Adds a command to the set of pending commands.
    /// </summary>
    /// <param name="executionData">The execution data describing the command to be added.</param>
    /// <returns><see langword="true"/> if the command was successfully added; otherwise <see langword="false"/>.</returns>
    protected bool AddPendingCommand(Command executionData)
    {
        return this.pendingCommands.TryAdd(executionData.CommandId, executionData);
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
        if (this.EventReceived is not null)
        {
            this.EventReceived(this, e);
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
        }

        if (message is not null)
        {
            if (message.RootElement.TryGetProperty("type", out JsonElement messageTypeToken) && messageTypeToken.ValueKind == JsonValueKind.String)
            {
                string messageType = messageTypeToken.GetString()!;
                if (messageType == "success")
                {
                    isProcessed = this.ProcessCommandResponseMessage(message.RootElement);
                }
                else if (messageType == "error")
                {
                    isProcessed = this.ProcessErrorMessage(message.RootElement);
                }
                else if (messageType == "event")
                {
                    isProcessed = this.ProcessEventMessage(message.RootElement);
                }
            }
            else
            {
                // TODO: Remove this else clause when the browser stable channels
                // have the message type property implemented.
                if (message.RootElement.TryGetProperty("error", out JsonElement errorToken) && errorToken.ValueKind == JsonValueKind.String)
                {
                    isProcessed = this.ProcessErrorMessage(message.RootElement);
                }
                else
                {
                    if (message.RootElement.TryGetProperty("id", out JsonElement idToken) && idToken.TryGetInt64(out long _))
                    {
                        isProcessed = this.ProcessCommandResponseMessage(message.RootElement);
                    }
                    else
                    {
                        if (message.RootElement.TryGetProperty("method", out JsonElement eventNameToken) && eventNameToken.ValueKind == JsonValueKind.String)
                        {
                            isProcessed = this.ProcessEventMessage(message.RootElement);
                        }
                    }
                }
            }
        }

        if (!isProcessed)
        {
            this.OnProtocolUnknownMessageReceived(this, new UnknownMessageReceivedEventArgs(messageData));
        }
    }

    private bool ProcessCommandResponseMessage(JsonElement message)
    {
        if (message.TryGetProperty("id", out JsonElement idToken) && idToken.ValueKind == JsonValueKind.Number && idToken.TryGetInt64(out long responseId))
        {
             if (this.pendingCommands.TryGetValue(responseId, out Command? executedCommand))
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
                if (errorMessage.CommandId.HasValue && this.pendingCommands.TryGetValue(errorMessage.CommandId.Value, out Command? executedCommand))
                {
                    executedCommand.Result = errorMessage.GetErrorResponseData();
                }
                else
                {
                    this.OnProtocolErrorEventReceived(this, new ErrorReceivedEventArgs(errorMessage.GetErrorResponseData()));
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            this.Log($"Unexpected error parsing error JSON: {ex.Message} (JSON: {message})", WebDriverBiDiLogLevel.Error);
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
            if (this.eventMessageTypes.TryGetValue(eventName, out Type? eventMessageType))
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
                }
            }
        }

        return false;
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
}
