namespace WebDriverBidi.TestUtilities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class TestDriver : Driver
{
    private long nextCommandId = 0;
    private WebDriverBidiCommand? lastCommand;

    private readonly TimeSpan commandTimeout = TimeSpan.FromSeconds(5);

    private readonly AutoResetEvent commandSetEvent = new(false);

    private readonly Dictionary<string, Type> eventTypes = new();

    private readonly TestProtocolModule testModule;

    private bool emitNullEventArgs = false;

    public TestDriver()
    {
        this.testModule = new(this);
    }

    public TestDriver(ProtocolTransport transport)
        : base(transport)
    {
        this.testModule = new(this);
    }

    public event EventHandler<TestCommandSetEventArgs>? CommandSet;

    public TestProtocolModule Test => this.testModule;

    public bool EmitNullEventArgs { get => this.emitNullEventArgs; set => this.emitNullEventArgs = value; }

    public WebDriverBidiCommand? LastCommand => this.lastCommand;

    public void EmitResponse(string jsonResponse)
    {
        this.ProcessMessage(jsonResponse);
    }

    public void WaitForCommandSet(TimeSpan waitTimeout)
    {
        this.commandSetEvent.WaitOne(waitTimeout);
    }

    public override void RegisterEvent(string eventName, Type eventArgsType)
    {
        this.eventTypes[eventName] = eventArgsType;
    }

    public override async Task<T> ExecuteCommand<T>(CommandData command)
    {
        long commandId  = Interlocked.Increment(ref this.nextCommandId);
        this.lastCommand = new WebDriverBidiCommand(commandId, command);
        this.commandSetEvent.Set();
        this.OnCommandSet(new TestCommandSetEventArgs(command.MethodName, command.ResponseType));
        await this.WaitForCommandComplete();
        var result = this.lastCommand.Result;
        this.lastCommand = null;
        if (result is null)
        {
            throw new WebDriverBidiException("Received null response from transport for SendCommandAndWait");
        }

        if (result.IsError)
        {
            if (result is not ErrorResponseData errorResponse)
            {
                throw new WebDriverBidiException("Received null converting error response from transport for SendCommandAndWait");
            }

            throw new WebDriverBidiException($"Received '{errorResponse.ErrorType}' error executing command {command.MethodName}: {errorResponse.ErrorMessage}");
        }

        if (result is not T convertedResult)
        {
            throw new WebDriverBidiException("Received null converting response from transport for SendCommandAndWait");
        }

        return await Task.FromResult<T>(convertedResult);
    }

    protected override void OnEventReceived(ProtocolEventReceivedEventArgs e)
    {
        base.OnEventReceived(e);
    }

    protected override void OnUnexpectedError(ProtocolErrorReceivedEventArgs e)
    {
        base.OnUnexpectedError(e);
    }

    protected virtual void OnCommandSet(TestCommandSetEventArgs e)
    {
        if (this.CommandSet is not null)
        {
            this.CommandSet(this, e);
        }
    }

    private void ProcessMessage(string messageData)
    {
        JObject message = JObject.Parse(messageData);
        if (message.ContainsKey("method") && message.ContainsKey("params"))
        {
            // This is an event
            string? eventName = message["method"]!.Value<string>();
            JToken? eventData = message["params"];
            if (eventName is not null && eventData is not null && this.eventTypes.ContainsKey(eventName))
            {
                Type eventType = eventTypes[eventName];
                object? eventDataObject = null;
                try
                {
                    eventDataObject = eventData.ToObject(eventType);
                }
                catch (JsonSerializationException)
                {
                    if (!this.emitNullEventArgs)
                    {
                        eventDataObject = "This is an invalid, non-null event data object";
                    }
                }
                
                this.OnEventReceived(new ProtocolEventReceivedEventArgs(eventName, eventDataObject));
            }
        }
        else if (this.lastCommand is not null)
        {
            if (message.ContainsKey("result"))
            {
                var response = message.ToObject(lastCommand.ResponseType) as CommandResponse;
                this.lastCommand.Result = response!.Result;
            }
            else if (message.ContainsKey("error"))
            {
                this.lastCommand.Result = message.ToObject<ErrorResponseData>();
            }

            this.lastCommand.SynchronizationEvent.Set();
        }
        else
        {
            // This is an error response, not connected to a command.
            this.OnUnexpectedError(new ProtocolErrorReceivedEventArgs(message.ToObject<ErrorResponseData>()));
        }
    }

    private async Task WaitForCommandComplete()
    {
        if (this.lastCommand is null)
        {
            return;            
        }

        var timeout = DateTime.Now.Add(this.commandTimeout);
        bool isRunning = !this.lastCommand.SynchronizationEvent.WaitOne(TimeSpan.Zero);
        while (isRunning && DateTime.Now < timeout)
        {
            await Task.Delay(50);
            isRunning = !this.lastCommand.SynchronizationEvent.WaitOne(TimeSpan.Zero);
        }
    }
}