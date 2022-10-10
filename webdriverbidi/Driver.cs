namespace WebDriverBidi;

using Newtonsoft.Json.Linq;
using BrowsingContext;
using Log;
using Script;
using Session;

public class Driver
{
    private ProtocolTransport transport;
    private BrowsingContextModule browsingContextModule;
    private SessionModule sessionModule;
    private ScriptModule scriptModule;
    private LogModule logModule;

    public Driver() : this(new ProtocolTransport())
    {
    }

    public Driver(ProtocolTransport transport)
    {
        this.transport = transport;
        this.transport.EventReceived += OnTransportEventReceived;
        this.transport.ErrorEventReceived += OnTransportErrorEventReceived;
        this.transport.LogMessage += OnTransportLogMessage;
        this.browsingContextModule = new BrowsingContextModule(this);
        this.sessionModule = new SessionModule(this);
        this.scriptModule = new ScriptModule(this);
        this.logModule = new LogModule(this);
    }

    public event EventHandler<ProtocolEventReceivedEventArgs>? EventReceived;

    public event EventHandler<ProtocolErrorReceivedEventArgs>? UnexpectedErrorReceived;

    public event EventHandler<LogMessageEventArgs>? LogMessage;

    public BrowsingContextModule BrowsingContext => this.browsingContextModule;

    public SessionModule Session => this.sessionModule;

    public ScriptModule Script => this.scriptModule;

    public LogModule Log => this.logModule;

    public virtual async Task Start(string url)
    {
        await transport.Connect(url);
    }

    public virtual async Task Stop()
    {
        await transport.Disconnect();
    }

    public virtual async Task<T> ExecuteCommand<T>(CommandSettings command) where T : CommandResult
    {
        var result = await transport.SendCommandAndWait(command.MethodName, JToken.FromObject(command));
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

        return convertedResult;
    }

    protected virtual void OnUnexpectedError(ProtocolErrorReceivedEventArgs e)
    {
        if (this.UnexpectedErrorReceived is not null)
        {
            this.UnexpectedErrorReceived(this, e);
        }
    }

    protected virtual void OnEventReceived(ProtocolEventReceivedEventArgs e)
    {
        if (this.EventReceived is not null)
        {
            this.EventReceived(this, e);
        }
    }

    protected virtual void OnLogMessage(LogMessageEventArgs e)
    {
        if (this.LogMessage is not null)
        {
            this.LogMessage(this, e);
        }
    }

    private void OnTransportEventReceived(object? sender, ProtocolEventReceivedEventArgs e)
    {
        this.OnEventReceived(e);
    }

    private void OnTransportErrorEventReceived(object? sender, ProtocolErrorReceivedEventArgs e)
    {
        this.OnUnexpectedError(e);
    }

    private void OnTransportLogMessage(object? sender, LogMessageEventArgs e)
    {
        this.OnLogMessage(e);
    }
}