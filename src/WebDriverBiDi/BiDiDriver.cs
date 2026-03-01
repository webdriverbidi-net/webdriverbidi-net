// <copyright file="BiDiDriver.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Text.Json.Serialization.Metadata;
using WebDriverBiDi.Bluetooth;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Permissions;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Speculation;
using WebDriverBiDi.Storage;
using WebDriverBiDi.UserAgentClientHints;
using WebDriverBiDi.WebExtension;

/// <summary>
/// Object containing commands to drive a browser using the WebDriver BiDi protocol.
/// </summary>
public class BiDiDriver : IAsyncDisposable
{
    private const string EventReceivedEventName = "driver.eventReceived";
    private const string UnexpectedErrorReceivedEventName = "driver.unexpectedErrorReceived";
    private const string UnknownMessageReceivedEventName = "driver.unknownMessageReceived";
    private const string LogMessageEventName = "driver.logMessage";

    private readonly BluetoothModule bluetoothModule;
    private readonly BrowserModule browserModule;
    private readonly BrowsingContextModule browsingContextModule;
    private readonly EmulationModule emulationModule;
    private readonly InputModule inputModule;
    private readonly LogModule logModule;
    private readonly NetworkModule networkModule;
    private readonly PermissionsModule permissionsModule;
    private readonly ScriptModule scriptModule;
    private readonly SessionModule sessionModule;
    private readonly SpeculationModule speculationModule;
    private readonly StorageModule storageModule;
    private readonly UserAgentClientHintsModule userAgentClientHintsModule;
    private readonly WebExtensionModule webExtensionModule;

    private readonly TimeSpan defaultCommandWaitTimeout;
    private readonly Transport transport;
    private readonly ConcurrentDictionary<string, Module> modules = [];
    private readonly ConcurrentDictionary<string, EventInvoker> eventInvokers = [];
    private int isDisposedFlag = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="BiDiDriver" /> class.
    /// </summary>
    public BiDiDriver()
        : this(TimeSpan.FromMinutes(5), new Transport())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BiDiDriver" /> class with the specified
    /// default command wait timeout.
    /// </summary>
    /// <param name="defaultCommandWaitTimeout">The default timeout to wait for a command to complete.</param>
    public BiDiDriver(TimeSpan defaultCommandWaitTimeout)
        : this(defaultCommandWaitTimeout, new Transport())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BiDiDriver" /> class with the specified
    /// default command wait timeout and <see cref="Transport" />.
    /// </summary>
    /// <param name="defaultCommandWaitTimeout">The default timeout to wait for a command to complete.</param>
    /// <param name="transport">The protocol transport object used to communicate with the browser.</param>
    public BiDiDriver(TimeSpan defaultCommandWaitTimeout, Transport transport)
    {
        this.defaultCommandWaitTimeout = defaultCommandWaitTimeout;
        this.transport = transport;
        this.transport.OnEventReceived.AddObserver(this.OnTransportEventReceived);
        this.transport.OnErrorEventReceived.AddObserver(this.OnTransportErrorEventReceivedAsync);
        this.transport.OnUnknownMessageReceived.AddObserver(this.OnTransportUnknownMessageReceivedAsync);
        this.transport.OnLogMessage.AddObserver(this.OnTransportLogMessageAsync);

        this.bluetoothModule = new BluetoothModule(this);
        this.RegisterModule(this.bluetoothModule);

        this.browserModule = new BrowserModule(this);
        this.RegisterModule(this.browserModule);

        this.browsingContextModule = new BrowsingContextModule(this);
        this.RegisterModule(this.browsingContextModule);

        this.emulationModule = new EmulationModule(this);
        this.RegisterModule(this.emulationModule);

        this.inputModule = new InputModule(this);
        this.RegisterModule(this.inputModule);

        this.logModule = new LogModule(this);
        this.RegisterModule(this.logModule);

        this.networkModule = new NetworkModule(this);
        this.RegisterModule(this.networkModule);

        this.permissionsModule = new PermissionsModule(this);
        this.RegisterModule(this.permissionsModule);

        this.scriptModule = new ScriptModule(this);
        this.RegisterModule(this.scriptModule);

        this.sessionModule = new SessionModule(this);
        this.RegisterModule(this.sessionModule);

        this.speculationModule = new SpeculationModule(this);
        this.RegisterModule(this.speculationModule);

        this.storageModule = new StorageModule(this);
        this.RegisterModule(this.storageModule);

        this.userAgentClientHintsModule = new UserAgentClientHintsModule(this);
        this.RegisterModule(this.userAgentClientHintsModule);

        this.webExtensionModule = new WebExtensionModule(this);
        this.RegisterModule(this.webExtensionModule);
    }

    /// <summary>
    /// Gets an observable event that notifies when a protocol event is received from protocol transport.
    /// </summary>
    public ObservableEvent<EventReceivedEventArgs> OnEventReceived { get; } = new(EventReceivedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a protocol error is received from protocol transport.
    /// </summary>
    public ObservableEvent<ErrorReceivedEventArgs> OnUnexpectedErrorReceived { get; } = new(UnexpectedErrorReceivedEventName);

    /// <summary>
    /// Gets an observable event that notifies when an unknown message is received from protocol transport.
    /// </summary>
    public ObservableEvent<UnknownMessageReceivedEventArgs> OnUnknownMessageReceived { get; } = new(UnknownMessageReceivedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by this driver.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; } = new(LogMessageEventName);

    /// <summary>
    /// Gets the bluetooth module as described in the W3C Web Bluetooth Specification.
    /// </summary>
    public BluetoothModule Bluetooth => this.bluetoothModule;

    /// <summary>
    /// Gets the browser module as described in the WebDriver BiDi protocol.
    /// </summary>
    public BrowserModule Browser => this.browserModule;

    /// <summary>
    /// Gets the browsingContext module as described in the WebDriver BiDi protocol.
    /// </summary>
    public BrowsingContextModule BrowsingContext => this.browsingContextModule;

    /// <summary>
    /// Gets the emulation module as described in the WebDriver BiDi protocol.
    /// </summary>
    public EmulationModule Emulation => this.emulationModule;

    /// <summary>
    /// Gets the input module as described in the WebDriver BiDi protocol.
    /// </summary>
    public InputModule Input => this.inputModule;

    /// <summary>
    /// Gets the log module as described in the WebDriver BiDi protocol.
    /// </summary>
    public LogModule Log => this.logModule;

    /// <summary>
    /// Gets the network module as described in the WebDriver BiDi protocol.
    /// </summary>
    public NetworkModule Network => this.networkModule;

    /// <summary>
    /// Gets the permissions module as described in the W3C Permissions Specification.
    /// </summary>
    public PermissionsModule Permissions => this.permissionsModule;

    /// <summary>
    /// Gets the script module as described in the WebDriver BiDi protocol.
    /// </summary>
    public ScriptModule Script => this.scriptModule;

    /// <summary>
    /// Gets the session module as described in the WebDriver BiDi protocol.
    /// </summary>
    public SessionModule Session => this.sessionModule;

    /// <summary>
    /// Gets the speculation module as described in the W3C Community Group Prerendering specification.
    /// </summary>
    public SpeculationModule Speculation => this.speculationModule;

    /// <summary>
    /// Gets the storage module as described in the WebDriver BiDi protocol.
    /// </summary>
    public StorageModule Storage => this.storageModule;

    /// <summary>
    /// Gets the user agent client hints module as described in the W3C Community Group User Agent Client Hints specification.
    /// </summary>
    public UserAgentClientHintsModule UserAgentClientHints => this.userAgentClientHintsModule;

    /// <summary>
    /// Gets the web extension module as described in the WebDriver BiDi protocol.
    /// </summary>
    public WebExtensionModule WebExtension => this.webExtensionModule;

    /// <summary>
    /// Gets a value indicating whether this driver is disposed.
    /// Use this property to ensure thread-safe operations for checking disposal state.
    /// </summary>
    protected bool IsDisposed => Interlocked.CompareExchange(ref this.isDisposedFlag, 0, 0) == 1;

    /// <summary>
    /// Asynchronously starts the communication with the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <param name="url">The URL of the remote end.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task StartAsync(string url)
    {
        this.ThrowIfDisposed();
        await this.transport.ConnectAsync(url).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously stops the communication with the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task StopAsync()
    {
        await this.transport.DisconnectAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for the
    /// default command timeout. The result type is inferred from the command parameters.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="command">The object containing settings for the command, including parameters.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if an error occurs during the execution of the command.</exception>
    public virtual async Task<T> ExecuteCommandAsync<T>(CommandParameters<T> command)
        where T : CommandResult
    {
        return await this.ExecuteCommandAsync<T>(command, this.defaultCommandWaitTimeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for a response.
    /// The result type is inferred from the command parameters.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="command">The object containing settings for the command, including parameters.</param>
    /// <param name="commandTimeout">The timeout to wait for the command to complete.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if an error occurs during the execution of the command.</exception>
    public virtual async Task<T> ExecuteCommandAsync<T>(CommandParameters<T> command, TimeSpan commandTimeout)
        where T : CommandResult
    {
        return await this.ExecuteCommandAsync<T>((CommandParameters)command, commandTimeout).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for the
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
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for a response.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="command">The object containing settings for the command, including parameters.</param>
    /// <param name="commandTimeout">The timeout to wait for the command to complete.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiCommandException">Thrown if an error occurs during the execution of the command.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown if the command execution exceeds the specified timeout.</exception>
    /// <exception cref="WebDriverBiDiException">Thrown if the command is cancelled, returns a null value, or does not return a result of the correct object type.</exception>
    public virtual async Task<T> ExecuteCommandAsync<T>(CommandParameters command, TimeSpan commandTimeout)
        where T : CommandResult
    {
        this.ThrowIfDisposed();
        Command sentCommand = await this.transport.SendCommandAsync(command).ConfigureAwait(false);
        bool commandCompleted = await sentCommand.WaitForCompletionAsync(commandTimeout).ConfigureAwait(false);
        if (!commandCompleted)
        {
            this.transport.CancelCommand(sentCommand);
            throw new WebDriverBiDiTimeoutException($"Timed out executing command {command.MethodName} after {commandTimeout.TotalMilliseconds} milliseconds");
        }

        if (sentCommand.Result is null)
        {
            if (sentCommand.ThrownException is not null)
            {
                ExceptionDispatchInfo.Capture(sentCommand.ThrownException).Throw();
            }

            if (sentCommand.IsCanceled)
            {
                throw new WebDriverBiDiException($"Command {command.MethodName} with id {sentCommand.CommandId} was canceled before a result was received");
            }

            throw new WebDriverBiDiException($"Result for command {command.MethodName} with id {sentCommand.CommandId} is unexpectedly null");
        }

        CommandResult result = sentCommand.Result;
        if (result.IsError)
        {
            if (result is not ErrorResult errorResponse)
            {
                throw new WebDriverBiDiException("Could not convert error response from transport for SendCommandAndWait to ErrorResult");
            }

            throw new WebDriverBiDiCommandException($"Received '{errorResponse.ErrorType}' error executing command {command.MethodName}: {errorResponse.ErrorMessage}", errorResponse);
        }

        if (result is not T convertedResult)
        {
            throw new WebDriverBiDiException($"Could not convert response from transport for SendCommandAndWait to {typeof(T)}");
        }

        return convertedResult;
    }

    /// <summary>
    /// Registers an additional <see cref="IJsonTypeInfoResolver"/> for JSON serialization
    /// and deserialization. This allows custom types, such as those from user-defined modules,
    /// to be serialized in AOT scenarios where reflection-based serialization is unavailable.
    /// This method must be called before starting the driver.
    /// </summary>
    /// <param name="resolver">The type info resolver to add.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the driver has been disposed.</exception>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown if the driver has already been started.
    /// </exception>
    public virtual void RegisterTypeInfoResolver(IJsonTypeInfoResolver resolver)
    {
        this.ThrowIfDisposed();
        this.transport.RegisterTypeInfoResolver(resolver);
    }

    /// <summary>
    /// Registers a module for use with this driver.
    /// </summary>
    /// <param name="module">The module object.</param>
    /// <exception cref="ArgumentException">Thrown when attempting to register a module with a name tha has alreaey been registered.</exception>
    public virtual void RegisterModule(Module module)
    {
        if (!this.modules.TryAdd(module.ModuleName, module))
        {
            throw new ArgumentException($"A module with the name '{module.ModuleName}' has already been registered", nameof(module));
        }
    }

    /// <summary>
    /// Gets a module from the set of registered modules for this driver.
    /// </summary>
    /// <typeparam name="T">A module object which is a subclass of <see cref="Module"/>.</typeparam>
    /// <param name="moduleName">The name of the module to return.</param>
    /// <returns>The protocol module object.</returns>
    public virtual T GetModule<T>(string moduleName)
        where T : Module
    {
        if (!this.modules.TryGetValue(moduleName, out Module? module))
        {
            throw new WebDriverBiDiException($"Module '{moduleName}' is not registered with this driver");
        }

        if (module is not T)
        {
            throw new WebDriverBiDiException($"Module '{moduleName}' is registered with this driver, but the module object is not of type {typeof(T)}");
        }

        return (T)module;
    }

    /// <summary>
    /// Registers an event to be raised by the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <typeparam name="T">The type of data that will be raised by the event.</typeparam>
    /// <param name="eventName">The name of the event to raise.</param>
    /// <param name="eventInvoker">The delegate taking a single parameter of type T used to invoke the event.</param>
    public virtual void RegisterEvent<T>(string eventName, Func<EventInfo<T>, Task> eventInvoker)
    {
        this.eventInvokers[eventName] = new EventInvoker<T>(eventInvoker);
        this.transport.RegisterEventMessage<T>(eventName);
    }

    /// <summary>
    /// Asynchronously releases the resources used by this driver instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously releases the resources used by this driver instance.
    /// Override this method in derived classes to add custom cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!this.IsDisposed)
        {
            this.SetDisposed();
            try
            {
                await this.StopAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this.OnLogMessage.NotifyObserversAsync(
                    new LogMessageEventArgs(
                        $"Unexpected exception during disposal: {ex.Message}",
                        WebDriverBiDiLogLevel.Warn,
                        "BiDiDriver"));
            }

            await this.transport.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Marks this <see cref="BiDiDriver"/> as disposed. Use this method to ensure
    /// thread-safe operations for setting object being disposed.
    /// </summary>
    protected void SetDisposed()
    {
        Interlocked.Exchange(ref this.isDisposedFlag, 1);
    }

    private void ThrowIfDisposed()
    {
        if (this.IsDisposed)
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }
    }

    private async Task OnTransportEventReceived(EventReceivedEventArgs e)
    {
        if (this.eventInvokers.TryGetValue(e.EventName, out EventInvoker? invoker))
        {
            await invoker.InvokeEventAsync(e.EventData!, e.AdditionalData);
        }

        await this.OnEventReceived.NotifyObserversAsync(e);
    }

    private async Task OnTransportErrorEventReceivedAsync(ErrorReceivedEventArgs e)
    {
        await this.OnUnexpectedErrorReceived.NotifyObserversAsync(e);
    }

    private async Task OnTransportUnknownMessageReceivedAsync(UnknownMessageReceivedEventArgs e)
    {
        await this.OnUnknownMessageReceived.NotifyObserversAsync(e);
    }

    private async Task OnTransportLogMessageAsync(LogMessageEventArgs e)
    {
        await this.OnLogMessage.NotifyObserversAsync(e);
    }
}
