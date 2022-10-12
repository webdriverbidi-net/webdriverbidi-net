namespace WebDriverBidi.TestUtilities;

using Newtonsoft.Json.Linq;

public class TestDriver : Driver
{
    private long nextCommandId = 0;
    private WebDriverBidiCommandData? lastCommand;

    private TimeSpan commandTimeout = TimeSpan.FromSeconds(5);

    private AutoResetEvent commandSetEvent = new AutoResetEvent(false);

    private Dictionary<string, Type> eventTypes = new Dictionary<string, Type>();

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

    public override async Task<T> ExecuteCommand<T>(CommandSettings command)
    {
        long commandId  = Interlocked.Increment(ref this.nextCommandId);
        this.lastCommand = new WebDriverBidiCommandData(commandId, command);
        this.commandSetEvent.Set();
        await this.WaitForCommandComplete();
        var result = this.lastCommand.Result;
        this.lastCommand = null;
        if (result is null)
        {
            throw new WebDriverBidiException("Received null response from transport for SendCommandAndWait");
        }

        if (result.IsError)
        {
            ErrorResponse? errorResponse = result as ErrorResponse;
            if (errorResponse is null)
            {
                throw new WebDriverBidiException("Received null converting error response from transport for SendCommandAndWait");
            }

            throw new WebDriverBidiException($"Received '{errorResponse.ErrorType}' error executing command {command.MethodName}: {errorResponse.ErrorMessage}");
        }

        T? convertedResult = result as T;
        if (convertedResult is null)
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
                this.OnEventReceived(new ProtocolEventReceivedEventArgs(eventName, eventData.ToObject(eventType)));
            }
        }
        else if (this.lastCommand is not null)
        {
            if (message.ContainsKey("result"))
            {
                var result = message["result"]!.ToObject(lastCommand.ResultType) as CommandResult;
                this.lastCommand.Result = result;
            }
            else if (message.ContainsKey("error"))
            {
                this.lastCommand.Result = message.ToObject<ErrorResponse>();
            }

            this.lastCommand.SynchronizationEvent.Set();
        }
        else
        {
            // This is an error response, not connected to a command.
            this.OnUnexpectedError(new ProtocolErrorReceivedEventArgs(message.ToObject<ErrorResponse>()));
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