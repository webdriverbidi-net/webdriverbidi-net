// <copyright file="Transport.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Protocol;

using System.Collections.Concurrent;
using System.Threading.Channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// The transport object used for serializing and deserializing JSON data used in the WebDriver Bidi protocol.
/// </summary>
public class Transport
{
    private readonly ConcurrentDictionary<long, Command> pendingCommands = new();
    private readonly Connection connection;
    private readonly TimeSpan commandWaitTimeout;
    private readonly Dictionary<string, Type> eventMessageTypes = new();
    private readonly Channel<string> messageQueue;
    private readonly Task messageQueueMonitorTask;
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
        this.messageQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true,
        });
        this.messageQueueMonitorTask = Task.Run(() => this.MonitorMessageQueue());
        this.messageQueueMonitorTask.ConfigureAwait(false);
        this.commandWaitTimeout = commandWaitTimeout;
        this.connection = connection;
        connection.DataReceived += this.OnMessageReceived;
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
    public async Task Connect(string websocketUri)
    {
        if (!this.isConnected)
        {
            await this.connection.Start(websocketUri);
            this.isConnected = true;
        }
    }

    /// <summary>
    /// Asyncronously disconnects from the remote end web socket.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Disconnect()
    {
        await this.connection.Stop();
        await this.ShutdownMessageQueue();
        this.isConnected = false;
    }

    /// <summary>
    /// Asynchronously send a command to the remote end and waits for a response.
    /// </summary>
    /// <param name="command">The command settings object containing all data required to execute the command.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task<CommandResult> SendCommandAndWait(CommandParameters command)
    {
        long commandId = await this.SendCommand(command);
        this.WaitForCommandComplete(commandId, this.commandWaitTimeout);
        return this.GetCommandResponse(commandId);
    }

    /// <summary>
    /// Asynchonously sends a command to the remote end.
    /// </summary>
    /// <param name="command">The command settings object containing all data required to execute the command.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBidiException">Thrown if the command ID is already in use.</exception>
    public virtual async Task<long> SendCommand(CommandParameters command)
    {
        long commandId = Interlocked.Increment(ref this.nextCommandId);
        Command executionData = new(commandId, command);
        if (!this.AddPendingCommand(executionData))
        {
            throw new WebDriverBidiException($"Could not add command with id {executionData.CommandId}, as id already exists");
        }

        await this.connection.SendData(JsonConvert.SerializeObject(executionData));
        return executionData.CommandId;
    }

    /// <summary>
    /// Waits for a command with the given ID to complete.
    /// </summary>
    /// <param name="commandId">The ID of the command for which to wait for completion.</param>
    /// <param name="waitTimeout">The timeout describing how long to wait for the command to complete.</param>
    /// <exception cref="WebDriverBidiException">Thrown if the command ID is invalid or if the command times out.</exception>
    public virtual void WaitForCommandComplete(long commandId, TimeSpan waitTimeout)
    {
        if (!this.pendingCommands.ContainsKey(commandId))
        {
            throw new WebDriverBidiException($"Unknown command id {commandId}");
        }
        else
        {
            if (!this.pendingCommands[commandId].SynchronizationEvent.Wait(waitTimeout))
            {
                throw new WebDriverBidiException($"Timed out waiting for response for command id {commandId}");
            }
        }
    }

    /// <summary>
    /// Gets the result of the command.
    /// </summary>
    /// <param name="commandId">The ID of the command for which to get the response.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBidiException">Thrown if the command result could not be retreived, or if the command result is not valid.</exception>
    public virtual CommandResult GetCommandResponse(long commandId)
    {
        if (this.pendingCommands.TryRemove(commandId, out Command? command))
        {
            if (command.Result is null)
            {
                if (command.ThrownException is null)
                {
                    throw new WebDriverBidiException($"Result and thrown exception for command with id {commandId} are both null");
                }

                throw command.ThrownException;
            }

            return command.Result;
        }

        throw new WebDriverBidiException($"Could not remove command with id {commandId}");
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
    /// Raises the EventRecived event.
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
    /// Raises the ErrorEventRecieved event.
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

    /// <summary>
    /// Reads incoming messages in the message queue.
    /// </summary>
    protected virtual void ReadIncomingMessages()
    {
        while (this.messageQueue.Reader.TryRead(out string? message))
        {
            this.ProcessMessage(message);
        }
    }

    private void OnMessageReceived(object? sender, ConnectionDataReceivedEventArgs e)
    {
        _ = this.messageQueue.Writer.TryWrite(e.Data);
    }

    private async Task MonitorMessageQueue()
    {
        while (await this.messageQueue.Reader.WaitToReadAsync())
        {
            this.ReadIncomingMessages();
        }
    }

    private void ProcessMessage(string messageData)
    {
        bool isProcessed = false;
        JObject? message = null;
        try
        {
            message = JObject.Parse(messageData);
        }
        catch (JsonReaderException e)
        {
            this.Log($"Unexpected error parsing JSON message: {e.Message}", WebDriverBidiLogLevel.Error);
        }

        if (message is not null)
        {
            JToken? messageTypeToken = message["type"];
            if (messageTypeToken is not null && messageTypeToken.Type == JTokenType.String)
            {
                string messageType = messageTypeToken.Value<string>()!;
                if (messageType == "success")
                {
                    isProcessed = this.ProcessCommandResponseMessage(message);
                }
                else if (messageType == "error")
                {
                    isProcessed = this.ProcessErrorMessage(message);
                }
                else if (messageType == "event")
                {
                    isProcessed = this.ProcessEventMessage(message);
                }
            }
        }

        if (!isProcessed)
        {
            this.OnProtocolUnknownMessageReceived(this, new UnknownMessageReceivedEventArgs(messageData));
        }
    }

    private bool ProcessCommandResponseMessage(JObject message)
    {
        JToken? idToken = message["id"];
        if (idToken is not null && idToken.Type == JTokenType.Integer)
        {
            long responseId = idToken.Value<long>()!;
            if (this.pendingCommands.TryGetValue(responseId, out Command? executedCommand))
            {
                try
                {
                    if (message.ToObject(executedCommand.ResponseType) is CommandResponseMessage response)
                    {
                        executedCommand.Result = response.Result;
                        executedCommand.Result.AdditionalData = response.AdditionalData;
                    }
                }
                catch (Exception ex)
                {
                    executedCommand.ThrownException = new WebDriverBidiException($"Response did not contain properly formed JSON for response type (response JSON:{message})", ex);
                }

                executedCommand.SynchronizationEvent.Set();
                return true;
            }
        }

        return false;
    }

    private bool ProcessErrorMessage(JObject message)
    {
        try
        {
            // If the message doesn't match the schema of an actual error message,
            // an exception will be thrown by the JSON serializer, and we can log
            // the malformed response.
            ErrorResponseMessage errorMessage = message.ToObject<ErrorResponseMessage>()!;
            if (errorMessage.CommandId.HasValue && this.pendingCommands.TryGetValue(errorMessage.CommandId.Value, out Command? executedCommand))
            {
                executedCommand.Result = errorMessage.GetErrorResponseData();
                executedCommand.SynchronizationEvent.Set();
            }
            else
            {
                this.OnProtocolErrorEventReceived(this, new ErrorReceivedEventArgs(errorMessage.GetErrorResponseData()));
            }

            return true;
        }
        catch (Exception ex)
        {
            this.Log($"Unexpected error parsing error JSON: {ex.Message} (JSON: {message})", WebDriverBidiLogLevel.Error);
        }

        return false;
    }

    private bool ProcessEventMessage(JObject message)
    {
        JToken? eventNameToken = message["method"];
        if (eventNameToken is not null && eventNameToken.Type == JTokenType.String)
        {
            string eventName = eventNameToken.Value<string>()!;
            if (this.eventMessageTypes.TryGetValue(eventName, out Type? eventMessageType))
            {
                try
                {
                    // If the message doesn't match the schema of the specified event args type,
                    // an exception will be thrown by the JSON serializer, and we can log
                    // the malformed response.
                    EventMessage? eventMessageData = message.ToObject(eventMessageType) as EventMessage;
                    this.OnProtocolEventReceived(this, new EventReceivedEventArgs(eventMessageData!));
                    return true;
                }
                catch (Exception ex)
                {
                    this.Log($"Unexpected error parsing event JSON: {ex.Message} (JSON: {message})", WebDriverBidiLogLevel.Error);
                }
            }
        }

        return false;
    }

    private async Task ShutdownMessageQueue()
    {
        // Attempt to wait for the channel to empty before marking the
        // writer as complete and waiting for the monitor task to end.
        while (this.messageQueue.Reader.TryPeek(out _))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        this.messageQueue.Writer.Complete();
        this.messageQueueMonitorTask.Wait();
    }

    private void Log(string message, WebDriverBidiLogLevel level)
    {
        this.OnLogMessage(this, new LogMessageEventArgs(message, level, "Transport"));
    }

    private void OnConnectionLogMessage(object? sender, LogMessageEventArgs e)
    {
        this.OnLogMessage(sender, e);
    }
}