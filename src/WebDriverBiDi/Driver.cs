// <copyright file="Driver.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Object containing commands to drive a browser using the WebDriver Bidi protocol.
/// </summary>
public class Driver
{
    private readonly TimeSpan defaultCommandWaitTimeout;
    private readonly Transport transport;
    private readonly Dictionary<string, Module> modules = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Driver" /> class.
    /// </summary>
    public Driver()
        : this(Timeout.InfiniteTimeSpan, new Transport())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Driver" /> class with the specified
    /// default command wait timeout.
    /// </summary>
    /// <param name="defaultCommandWaitTimeout">The default timeout to wait for a command to complete.</param>
    public Driver(TimeSpan defaultCommandWaitTimeout)
        : this(defaultCommandWaitTimeout, new Transport())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Driver" /> class with the specified
    /// default command wait timeout and <see cref="Transport" />.
    /// </summary>
    /// <param name="defaultCommandWaitTimeout">The default timeout to wait for a command to complete.</param>
    /// <param name="transport">The protocol transport object used to communicate with the browser.</param>
    public Driver(TimeSpan defaultCommandWaitTimeout, Transport transport)
    {
        this.defaultCommandWaitTimeout = defaultCommandWaitTimeout;
        this.transport = transport;
        this.transport.EventReceived += this.OnTransportEventReceived;
        this.transport.ErrorEventReceived += this.OnTransportErrorEventReceived;
        this.transport.UnknownMessageReceived += this.OnTransportUnknownMessageReceived;
        this.transport.LogMessage += this.OnTransportLogMessage;
        this.RegisterModule(new BrowserModule(this));
        this.RegisterModule(new BrowsingContextModule(this));
        this.RegisterModule(new SessionModule(this));
        this.RegisterModule(new ScriptModule(this));
        this.RegisterModule(new LogModule(this));
        this.RegisterModule(new InputModule(this));
        this.RegisterModule(new NetworkModule(this));
    }

    /// <summary>
    /// Raised when a protocol event is received from protocol transport.
    /// </summary>
    public event EventHandler<EventReceivedEventArgs>? EventReceived;

    /// <summary>
    /// Raised when a protocol error is received from protocol transport.
    /// </summary>
    public event EventHandler<ErrorReceivedEventArgs>? UnexpectedErrorReceived;

    /// <summary>
    /// Raised when an unknown message is received from protocol transport.
    /// </summary>
    public event EventHandler<UnknownMessageReceivedEventArgs>? UnknownMessageReceived;

    /// <summary>
    /// Raised when a log message is emitted by this driver.
    /// </summary>
    public event EventHandler<LogMessageEventArgs>? LogMessage;

    /// <summary>
    /// Gets the browser module as described in the WebDriver Bidi protocol.
    /// </summary>
    public BrowserModule Browser => this.GetModule<BrowserModule>(BrowserModule.BrowserModuleName);

    /// <summary>
    /// Gets the browsingContext module as described in the WebDriver Bidi protocol.
    /// </summary>
    public BrowsingContextModule BrowsingContext => this.GetModule<BrowsingContextModule>(BrowsingContextModule.BrowsingContextModuleName);

    /// <summary>
    /// Gets the session module as described in the WebDriver Bidi protocol.
    /// </summary>
    public SessionModule Session => this.GetModule<SessionModule>(SessionModule.SessionModuleName);

    /// <summary>
    /// Gets the script module as described in the WebDriver Bidi protocol.
    /// </summary>
    public ScriptModule Script => this.GetModule<ScriptModule>(ScriptModule.ScriptModuleName);

    /// <summary>
    /// Gets the log module as described in the WebDriver Bidi protocol.
    /// </summary>
    public LogModule Log => this.GetModule<LogModule>(LogModule.LogModuleName);

    /// <summary>
    /// Gets the input module as described in the WebDriver Bidi protocol.
    /// </summary>
    public InputModule Input => this.GetModule<InputModule>(InputModule.InputModuleName);

    /// <summary>
    /// Gets the network module as described in the WebDriver Bidi protocol.
    /// </summary>
    public NetworkModule Network => this.GetModule<NetworkModule>(NetworkModule.NetworkModuleName);

    /// <summary>
    /// Asynchronously starts the communication with the remote end of the WebDriver Bidi protocol.
    /// </summary>
    /// <param name="url">The URL of the remote end.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task StartAsync(string url)
    {
        await this.transport.ConnectAsync(url).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously stops the communication with the remote end of the WebDriver Bidi protocol.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task StopAsync()
    {
        await this.transport.DisconnectAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver Bidi protocol and waits for the
    /// default command timeout.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="command">The object containing settings for the command, including parameters.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if an error occurs during the execution of the command.</exception>
    public virtual async Task<T> ExecuteCommandAsync<T>(CommandParameters command)
        where T : CommandResult
    {
        return await this.ExecuteCommandAsync<T>(command, this.defaultCommandWaitTimeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver Bidi protocol and waits for a response.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="command">The object containing settings for the command, including parameters.</param>
    /// <param name="commandTimeout">The timeout to wait for the command to complete.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if an error occurs during the execution of the command.</exception>
    public virtual async Task<T> ExecuteCommandAsync<T>(CommandParameters command, TimeSpan commandTimeout)
        where T : CommandResult
    {
        Command sentCommand = await this.transport.SendCommandAsync(command).ConfigureAwait(false);
        bool commandCompleted = await sentCommand.WaitForCompletionAsync(commandTimeout).ConfigureAwait(false);
        if (!commandCompleted)
        {
            throw new WebDriverBiDiException($"Timed out executing command {command.MethodName} after ${commandTimeout.TotalMilliseconds} milliseconds");
        }

        if (sentCommand.Result is null)
        {
            if (sentCommand.ThrownException is null)
            {
                throw new WebDriverBiDiException($"Result and thrown exception for command {command.MethodName} with id {sentCommand.CommandId} are both null");
            }

            throw sentCommand.ThrownException;
        }

        CommandResult result = sentCommand.Result;
        if (result.IsError)
        {
            if (result is not ErrorResult errorResponse)
            {
                throw new WebDriverBiDiException("Could not convert error response from transport for SendCommandAndWait to ErrorResult");
            }

            throw new WebDriverBiDiException($"Received '{errorResponse.ErrorType}' error executing command {command.MethodName}: {errorResponse.ErrorMessage}");
        }

        if (result is not T convertedResult)
        {
            throw new WebDriverBiDiException($"Could not convert response from transport for SendCommandAndWait to {typeof(T)}");
        }

        return convertedResult;
    }

    /// <summary>
    /// Registers a module for use with this driver.
    /// </summary>
    /// <param name="module">The module object.</param>
    public void RegisterModule(Module module)
    {
        this.modules[module.ModuleName] = module;
    }

    /// <summary>
    /// Gets a module from the set of registered modules for this driver.
    /// </summary>
    /// <typeparam name="T">A module object which is a subclass of <see cref="Module"/>.</typeparam>
    /// <param name="moduleName">The name of the module to return.</param>
    /// <returns>The protocol module object.</returns>
    public T GetModule<T>(string moduleName)
        where T : Module
    {
        if (!this.modules.ContainsKey(moduleName))
        {
            throw new WebDriverBiDiException($"Module '{moduleName}' is not registered with this driver");
        }

        Module module = this.modules[moduleName];
        if (module is not T)
        {
            throw new WebDriverBiDiException($"Module '{moduleName}' is registered with this driver, but the module object is not of type {typeof(T)}");
        }

        return (T)module;
    }

    /// <summary>
    /// Registers an event to be raised by the remote end of the WebDriver Bidi protocol.
    /// </summary>
    /// <typeparam name="T">The type of data that will be raised by the event.</typeparam>
    /// <param name="eventName">The name of the event to raise.</param>
    public virtual void RegisterEvent<T>(string eventName)
    {
        this.transport.RegisterEventMessage<T>(eventName);
    }

    /// <summary>
    /// Raises the UnexpectedErrorReceived event.
    /// </summary>
    /// <param name="e">The event args used when raising the event.</param>
    protected virtual void OnUnexpectedError(ErrorReceivedEventArgs e)
    {
        if (this.UnexpectedErrorReceived is not null)
        {
            this.UnexpectedErrorReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the EventReceived event.
    /// </summary>
    /// <param name="e">The event args used when raising the event.</param>
    protected virtual void OnEventReceived(EventReceivedEventArgs e)
    {
        if (this.EventReceived is not null)
        {
            this.EventReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the UnknownMessageReceived event.
    /// </summary>
    /// <param name="e">The event args used when raising the event.</param>
    protected virtual void OnUnknownMessageReceived(UnknownMessageReceivedEventArgs e)
    {
        if (this.UnknownMessageReceived is not null)
        {
            this.UnknownMessageReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the LogMessage event.
    /// </summary>
    /// <param name="e">The event args used when raising the event.</param>
    protected virtual void OnLogMessage(LogMessageEventArgs e)
    {
        if (this.LogMessage is not null)
        {
            this.LogMessage(this, e);
        }
    }

    private void OnTransportEventReceived(object? sender, EventReceivedEventArgs e)
    {
        this.OnEventReceived(e);
    }

    private void OnTransportErrorEventReceived(object? sender, ErrorReceivedEventArgs e)
    {
        this.OnUnexpectedError(e);
    }

    private void OnTransportUnknownMessageReceived(object? sender, UnknownMessageReceivedEventArgs e)
    {
        this.OnUnknownMessageReceived(e);
    }

    private void OnTransportLogMessage(object? sender, LogMessageEventArgs e)
    {
        this.OnLogMessage(e);
    }
}