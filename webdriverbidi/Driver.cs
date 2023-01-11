namespace WebDriverBidi;

using Newtonsoft.Json.Linq;
using BrowsingContext;
using Log;
using Script;
using Session;

public class Driver
{
    private readonly ProtocolTransport transport;
    private readonly BrowsingContextModule browsingContextModule;
    private readonly SessionModule sessionModule;
    private readonly ScriptModule scriptModule;
    private readonly LogModule logModule;

    public Driver() : this(new ProtocolTransport())
    {
    }

    public Driver(ProtocolTransport transport)
    {
        this.transport = transport;
        this.transport.EventReceived += OnTransportEventReceived;
        this.transport.ErrorEventReceived += OnTransportErrorEventReceived;
        this.transport.UnknownMessageReceived += OnTransportUnknownMessageReceived;
        this.transport.LogMessage += OnTransportLogMessage;
        this.browsingContextModule = new BrowsingContextModule(this);
        this.sessionModule = new SessionModule(this);
        this.scriptModule = new ScriptModule(this);
        this.logModule = new LogModule(this);
    }

    public event EventHandler<ProtocolEventReceivedEventArgs>? EventReceived;

    public event EventHandler<ProtocolErrorReceivedEventArgs>? UnexpectedErrorReceived;

    public event EventHandler<ProtocolUnknownMessageReceivedEventArgs>? UnknownMessageReceived;

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
        var result = await transport.SendCommandAndWait(command);
        if (result is null)
        {
            throw new WebDriverBidiException("Received null response from transport for SendCommandAndWait");
        }

        if (result.IsError)
        {
            if (result is not ErrorResponse errorResponse)
            {
                throw new WebDriverBidiException("Received null converting error response from transport for SendCommandAndWait");
            }

            throw new WebDriverBidiException($"Received '{errorResponse.ErrorType}' error executing command {command.MethodName}: {errorResponse.ErrorMessage}");
        }

        if (result is not T convertedResult)
        {
            throw new WebDriverBidiException("Received null converting response from transport for SendCommandAndWait");
        }

        return convertedResult;
    }

    public virtual void RegisterEvent(string eventName, Type eventArgsType)
    {
        this.transport.RegisterEvent(eventName, eventArgsType);
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

    protected virtual void OnUnknownMessageReceived(ProtocolUnknownMessageReceivedEventArgs e)
    {
        if (this.UnknownMessageReceived is not null)
        {
            this.UnknownMessageReceived(this, e);
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

    private void OnTransportUnknownMessageReceived(object? sender, ProtocolUnknownMessageReceivedEventArgs e)
    {
        this.OnUnknownMessageReceived(e);
    }

    private void OnTransportLogMessage(object? sender, LogMessageEventArgs e)
    {
        this.OnLogMessage(e);
    }
}