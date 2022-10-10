namespace WebDriverBidi.TestUtilities;

using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;

public class TestDriver : Driver
{
    private long nextCommandId = 0;
    private WebDriverBidiCommandData? lastCommand;

    private TimeSpan commandTimeout = TimeSpan.FromSeconds(5);

    private AutoResetEvent commandSetEvent = new AutoResetEvent(false);

    public void EmitResponse(string jsonResponse)
    {
        this.ProcessMessage(jsonResponse);
    }

    public void WaitForCommandSet(TimeSpan waitTimeout)
    {
        this.commandSetEvent.WaitOne(waitTimeout);
    }

    public override async Task<T> ExecuteCommand<T>(CommandSettings command)
    {
        long commandId  = Interlocked.Increment(ref this.nextCommandId);
        this.lastCommand = new WebDriverBidiCommandData(commandId, command.MethodName, JToken.FromObject(command));
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
            ErrorResponse? errorResponse = result.Result.ToObject<ErrorResponse>();
            if (errorResponse is null)
            {
                throw new WebDriverBidiException("Received null converting error response from transport for SendCommandAndWait");
            }

            throw new WebDriverBidiException($"Received '{errorResponse.ErrorType}' error executing command {command.MethodName}: {errorResponse.ErrorMessage}");
        }

        T? convertedResult = result.Result.ToObject<T>();
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
            if (eventName is not null && eventData is not null)
            {
                this.OnEventReceived(new ProtocolEventReceivedEventArgs(eventName, eventData));
            }
        }
        else if (this.lastCommand is not null)
        {
            if (message.ContainsKey("result"))
            {
                var result = new WebDriverBidiCommandResultData(message["result"]!, false);
                this.lastCommand.Result = result;
            }
            else if (message.ContainsKey("error"))
            {
                this.lastCommand.Result = new WebDriverBidiCommandResultData(message, true);
            }

            this.lastCommand.SynchronizationEvent.Set();
        }
        else
        {
            // This is an error response, not connected to a command.
            this.OnUnexpectedError(new ProtocolErrorReceivedEventArgs(message));
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