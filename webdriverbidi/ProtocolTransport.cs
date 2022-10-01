namespace WebDriverBidi;

using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ProtocolTransport
{
    private long nextCommandId = 0;
    private ConcurrentDictionary<long, WebDriverBidiCommandData> pendingCommands = new ConcurrentDictionary<long, WebDriverBidiCommandData>();
    private Connection connection;
    private TimeSpan commandWaitTimeout;

    public ProtocolTransport() : this(Timeout.InfiniteTimeSpan)
    {
    }

    public ProtocolTransport(TimeSpan commandWaitTimeout) : this(commandWaitTimeout, new Connection())
    {
    }

    public ProtocolTransport(TimeSpan commandWaitTimeout, Connection connection)
    {
        this.commandWaitTimeout = commandWaitTimeout;
        this.connection = connection;
        connection.DataReceived += this.OnMessageReceived;
        connection.LogMessage += this.OnConnectionLogMessage;
    }

    public event EventHandler<LogMessageEventArgs>? LogMessage;

    public event EventHandler<ProtocolEventReceivedEventArgs>? EventReceived;

    public event EventHandler<ProtocolErrorReceivedEventArgs>? ErrorEventReceived;

    public event EventHandler<ProtocolUnknownMessageReceivedEventArgs>? UnknownMessageReceived;

    public async Task Connect(string websocketUri)
    {
        await this.connection.Start(websocketUri);
    }

    public async Task Disconnect()
    {
        await this.connection.Stop();
    }

    public async Task<WebDriverBidiCommandResultData> SendCommandAndWait(string commandName, JToken commandParameters)
    {
        long commandId = await SendCommand(commandName, commandParameters);
        this.WaitForCommandComplete(commandId, this.commandWaitTimeout);
        return this.GetCommandResponse(commandId);
    }

    public async Task<long> SendCommand(string commandName, JToken commandParameters)
    {
        long commandId  = Interlocked.Increment(ref this.nextCommandId);
        var command = new WebDriverBidiCommandData(commandId, commandName, commandParameters);
        if (!this.pendingCommands.TryAdd(command.CommandId, command))
        {
            throw new WebDriverBidiException($"Could not add command with id {command.CommandId}, as id already exists");
        }

        await this.connection.SendData(JsonConvert.SerializeObject(command));
        return command.CommandId;
    }

    public void WaitForCommandComplete(long commandId, TimeSpan waitTimeout)
    {
        if (!this.pendingCommands.ContainsKey(commandId))
        {
            throw new WebDriverBidiException($"Unknown command id {commandId}");
        }
        else
        {
            if (!this.pendingCommands[commandId].SynchronizationEvent.WaitOne(waitTimeout))
            {
                throw new WebDriverBidiException($"Timed out waiting for response for command id {commandId}");
            }
        }
    }

    public WebDriverBidiCommandResultData GetCommandResponse(long commandId)
    {
        WebDriverBidiCommandData? command;
        if (this.pendingCommands.TryRemove(commandId, out command))
        {
            if (command.Result is null)
            {
                throw new WebDriverBidiException($"Result for command with id {commandId} is null");
            }

            return command.Result;
        }

        throw new WebDriverBidiException($"Could not remove command with id {commandId}");
    }

    protected virtual void OnLogMessage(object? sender, LogMessageEventArgs e)
    {
        if (this.LogMessage is not null)
        {
            this.LogMessage(this, e);
        }
    }

    protected virtual void OnProtocolEventReceived(object? sender, ProtocolEventReceivedEventArgs e)
    {
        if (this.EventReceived is not null)
        {
            this.EventReceived(this, e);
        }
    }

    protected virtual void OnProtocolErrorEventReceived(object? sender, ProtocolErrorReceivedEventArgs e)
    {
        if (this.ErrorEventReceived is not null)
        {
            this.ErrorEventReceived(this, e);
        }
    }

    protected virtual void OnProtocolUnknownMessageReceived(object? sender, ProtocolUnknownMessageReceivedEventArgs e)
    {
        if (this.UnknownMessageReceived is not null)
        {
            this.UnknownMessageReceived(this, e);
        }
    }

    private void OnMessageReceived(object? sender, DataReceivedEventArgs e)
    {
        // Ignore message that do not have either a "id" or a "message" property
        JObject message = JObject.Parse(e.Data);
        if (message.ContainsKey("id"))
        {
            JToken? idToken = message["id"];
            if (idToken is not null && idToken.Type != JTokenType.Null)
            {
                long? responseId = message["id"]!.Value<long>();
                if (this.pendingCommands.ContainsKey(responseId.Value))
                {
                    WebDriverBidiCommandData? executedCommand;
                    if (this.pendingCommands.TryGetValue(responseId.Value, out executedCommand))
                    {
                        if (message.ContainsKey("result"))
                        {
                            executedCommand.Result = new WebDriverBidiCommandResultData(message["result"]!, false);
                        }
                        else if (message.ContainsKey("error"))
                        {
                            executedCommand.Result = new WebDriverBidiCommandResultData(message, true);
                        }

                        executedCommand.SynchronizationEvent.Set();
                    }
                }
            }
            else if (message.ContainsKey("error"))
            {
                // This is an error response, not connected to a command.
                this.OnProtocolErrorEventReceived(this, new ProtocolErrorReceivedEventArgs(message));
            }
        }
        else if (message.ContainsKey("method") && message.ContainsKey("params"))
        {
            string? eventName = message["method"]!.Value<string>();
            JToken? eventData = message["params"];
            if (eventName is not null && eventData is not null)
            {
                this.OnProtocolEventReceived(this, new ProtocolEventReceivedEventArgs(eventName, eventData));
            }
        }
        else
        {
            this.OnProtocolUnknownMessageReceived(this, new ProtocolUnknownMessageReceivedEventArgs(message));
        }
    }

    private void OnConnectionLogMessage(object? sender, LogMessageEventArgs e)
    {
        this.OnLogMessage(sender, e);
    }
}