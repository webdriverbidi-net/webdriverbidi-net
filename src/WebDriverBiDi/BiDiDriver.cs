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
/// <remarks>
/// <para>
/// <strong>Thread Safety:</strong>
/// This class is thread-safe for concurrent command execution.
/// <see cref="IBiDiCommandExecutor.ExecuteCommandAsync{T}(CommandParameters{T}, TimeSpan?, CancellationToken)"/>
/// and module command methods may be called concurrently from multiple threads. Configuration operations
/// (<see cref="RegisterModule"/>, <see cref="RegisterEvent"/>, <see cref="RegisterTypeInfoResolverAsync"/>)
/// must complete before <see cref="StartAsync"/> is called and are serialized via an internal lock.
/// </para>
/// </remarks>
public class BiDiDriver : IBiDiCommandExecutor, IBiDiDriverConfiguration, IBiDiDriverEvents, IEventObserverErrorReporter
{
    /// <summary>
    /// Gets the the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "BiDiDriver";

    /// <summary>
    /// Gets the default command timeout if a timeout is not specified in the constructor.
    /// </summary>
    public static readonly TimeSpan DefaultCommandWaitTimeout = TimeSpan.FromSeconds(60);

    private const string EventReceivedEventName = "driver.eventReceived";
    private const string UnexpectedErrorReceivedEventName = "driver.unexpectedErrorReceived";
    private const string UnknownMessageReceivedEventName = "driver.unknownMessageReceived";
    private const string EventHandlerErrorOccurredEventName = "driver.eventHandlerErrorOccurred";
    private const string LogMessageEventName = "driver.logMessage";

    private readonly EventObserver<EventReceivedEventArgs> transportEventReceivedObserver;
    private readonly EventObserver<ErrorReceivedEventArgs> transportErrorReceivedObserver;
    private readonly EventObserver<UnknownMessageReceivedEventArgs> transportUnknownMessageReceivedObserver;
    private readonly EventObserver<LogMessageEventArgs> transportLogMessageObserver;
    private readonly EventObserver<EventHandlerErrorOccurredEventArgs> transportEventHandlerErrorOccurredObserver;

    private readonly Transport transport;
    private readonly ConcurrentDictionary<string, Module> modules = [];
    private readonly ConcurrentDictionary<string, EventInvoker> eventInvokers = [];
    private readonly object moduleRegistrationLock = new();
    private readonly object eventRegistrationLock = new();

    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required
    private int isDisposedFlag = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="BiDiDriver" /> class.
    /// </summary>
    public BiDiDriver()
        : this(DefaultCommandWaitTimeout, new Transport())
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
    /// <remarks>
    /// <para>
    /// This constructor is used when you need to provide a custom <see cref="Transport"/> instance,
    /// typically for advanced scenarios such as:
    /// <list type="bullet">
    /// <item><description>Using a custom <see cref="Connection"/> implementation with specific timeout configurations</description></item>
    /// <item><description>Sharing a transport across multiple driver instances (not recommended for typical usage)</description></item>
    /// <item><description>Integrating with specialized connection management frameworks</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Most users should use the simpler <see cref="BiDiDriver(TimeSpan)"/> constructor instead,
    /// which creates a default WebSocket-based transport automatically.
    /// </para>
    /// </remarks>
    public BiDiDriver(TimeSpan defaultCommandWaitTimeout, Transport transport)
    {
        if (transport is null)
        {
            throw new ArgumentNullException(nameof(transport), "The transport parameter must not be null");
        }

        if (defaultCommandWaitTimeout < TimeSpan.Zero && defaultCommandWaitTimeout != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultCommandWaitTimeout), "Default command wait timeout must be a non-negative TimeSpan value");
        }

        this.DefaultCommandTimeout = defaultCommandWaitTimeout;

        this.transport = transport;
        this.transportEventReceivedObserver = this.transport.OnEventReceived.AddObserver(this.OnTransportEventReceivedAsync);
        this.transportErrorReceivedObserver = this.transport.OnErrorEventReceived.AddObserver(this.OnTransportErrorEventReceivedAsync);
        this.transportUnknownMessageReceivedObserver = this.transport.OnUnknownMessageReceived.AddObserver(this.OnTransportUnknownMessageReceivedAsync);
        this.transportLogMessageObserver = this.transport.OnLogMessage.AddObserver(this.OnTransportLogMessageAsync);
        this.transportEventHandlerErrorOccurredObserver = this.transport.OnEventHandlerErrorOccurred.AddObserver(this.OnTransportEventHandlerErrorOccurredAsync);

        this.OnEventReceived = this.CreateObservableEvent<EventReceivedEventArgs>(EventReceivedEventName);
        this.OnUnexpectedErrorReceived = this.CreateObservableEvent<ErrorReceivedEventArgs>(UnexpectedErrorReceivedEventName);
        this.OnUnknownMessageReceived = this.CreateObservableEvent<UnknownMessageReceivedEventArgs>(UnknownMessageReceivedEventName);
        this.OnEventHandlerErrorOccurred = this.CreateObservableEvent<EventHandlerErrorOccurredEventArgs>(EventHandlerErrorOccurredEventName);
        this.OnLogMessage = this.CreateObservableEvent<LogMessageEventArgs>(LogMessageEventName);

        this.Bluetooth = new BluetoothModule(this);
        this.RegisterModule(this.Bluetooth);

        this.Browser = new BrowserModule(this);
        this.RegisterModule(this.Browser);

        this.BrowsingContext = new BrowsingContextModule(this);
        this.RegisterModule(this.BrowsingContext);

        this.Emulation = new EmulationModule(this);
        this.RegisterModule(this.Emulation);

        this.Input = new InputModule(this);
        this.RegisterModule(this.Input);

        this.Log = new LogModule(this);
        this.RegisterModule(this.Log);

        this.Network = new NetworkModule(this);
        this.RegisterModule(this.Network);

        this.Permissions = new PermissionsModule(this);
        this.RegisterModule(this.Permissions);

        this.Script = new ScriptModule(this);
        this.RegisterModule(this.Script);

        this.Session = new SessionModule(this);
        this.RegisterModule(this.Session);

        this.Speculation = new SpeculationModule(this);
        this.RegisterModule(this.Speculation);

        this.Storage = new StorageModule(this);
        this.RegisterModule(this.Storage);

        this.UserAgentClientHints = new UserAgentClientHintsModule(this);
        this.RegisterModule(this.UserAgentClientHints);

        this.WebExtension = new WebExtensionModule(this);
        this.RegisterModule(this.WebExtension);
    }

    /// <summary>
    /// Gets an observable event that notifies when a protocol event is received from protocol transport.
    /// </summary>
    public ObservableEvent<EventReceivedEventArgs> OnEventReceived { get; }

    /// <summary>
    /// Gets an observable event that notifies when a protocol error is received from protocol transport.
    /// </summary>
    public ObservableEvent<ErrorReceivedEventArgs> OnUnexpectedErrorReceived { get; }

    /// <summary>
    /// Gets an observable event that notifies when an unknown message is received from protocol transport.
    /// </summary>
    public ObservableEvent<UnknownMessageReceivedEventArgs> OnUnknownMessageReceived { get; }

    /// <summary>
    /// Gets an observable event that notifies when an error occurs in an observer of an observable event.
    /// </summary>
    /// <remarks>
    /// This event is for diagnostic and observability purposes, and does not prevent
    /// the propagation of the error back to the Transport class.
    /// </remarks>
    public ObservableEvent<EventHandlerErrorOccurredEventArgs> OnEventHandlerErrorOccurred { get; }

    /// <summary>
    /// Gets an observable event that notifies when a log message is emitted by this driver.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; }

    /// <summary>
    /// Gets the bluetooth module as described in the W3C Web Bluetooth Specification.
    /// </summary>
    public BluetoothModule Bluetooth { get; private set; }

    /// <summary>
    /// Gets the browser module as described in the WebDriver BiDi protocol.
    /// </summary>
    public BrowserModule Browser { get; private set; }

    /// <summary>
    /// Gets the browsingContext module as described in the WebDriver BiDi protocol.
    /// </summary>
    public BrowsingContextModule BrowsingContext { get; private set; }

    /// <summary>
    /// Gets the emulation module as described in the WebDriver BiDi protocol.
    /// </summary>
    public EmulationModule Emulation { get; private set; }

    /// <summary>
    /// Gets the input module as described in the WebDriver BiDi protocol.
    /// </summary>
    public InputModule Input { get; private set; }

    /// <summary>
    /// Gets the log module as described in the WebDriver BiDi protocol.
    /// </summary>
    public LogModule Log { get; private set; }

    /// <summary>
    /// Gets the network module as described in the WebDriver BiDi protocol.
    /// </summary>
    public NetworkModule Network { get; private set; }

    /// <summary>
    /// Gets the permissions module as described in the W3C Permissions Specification.
    /// </summary>
    public PermissionsModule Permissions { get; private set; }

    /// <summary>
    /// Gets the script module as described in the WebDriver BiDi protocol.
    /// </summary>
    public ScriptModule Script { get; private set; }

    /// <summary>
    /// Gets the session module as described in the WebDriver BiDi protocol.
    /// </summary>
    public SessionModule Session { get; private set; }

    /// <summary>
    /// Gets the speculation module as described in the W3C Community Group Prerendering specification.
    /// </summary>
    public SpeculationModule Speculation { get; private set; }

    /// <summary>
    /// Gets the storage module as described in the WebDriver BiDi protocol.
    /// </summary>
    public StorageModule Storage { get; private set; }

    /// <summary>
    /// Gets the user agent client hints module as described in the W3C Community Group User Agent Client Hints specification.
    /// </summary>
    public UserAgentClientHintsModule UserAgentClientHints { get; private set; }

    /// <summary>
    /// Gets the web extension module as described in the WebDriver BiDi protocol.
    /// </summary>
    public WebExtensionModule WebExtension { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the driver has started communication with the remote end of the WebDriver BiDi protocol.
    /// This property delegates to the underlying <see cref="Transport.IsConnected"/>, which in turn
    /// checks the <see cref="Connection.IsActive"/> state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property returns <see langword="true"/> after <see cref="StartAsync(string, CancellationToken)"/> completes successfully
    /// and remains <see langword="true"/> until <see cref="StopAsync(CancellationToken)"/> is called or the connection is lost.
    /// For WebSocket connections, this returns true when the WebSocket is in the Open state.
    /// For pipe connections, this returns true when both pipes are connected and the browser process is running.
    /// </para>
    /// <para>
    /// Use this property to check driver state before executing commands or during cleanup operations.
    /// </para>
    /// <para>
    /// <strong>Important timing restriction:</strong> Modules and event handlers must be registered
    /// <em>before</em> calling <see cref="StartAsync(string, CancellationToken)"/>. Attempting to call
    /// <see cref="RegisterModule(Module)"/> after the driver has started will throw an <see cref="InvalidOperationException"/>.
    /// </para>
    /// </remarks>
    public virtual bool IsStarted => this.transport.IsConnected;

    /// <summary>
    /// Gets the default timeout to wait for a command to complete. This timeout is specified in
    /// the constructor and is used by the <see cref="ExecuteCommandAsync{T}(CommandParameters{T}, TimeSpan?, CancellationToken)"/>
    /// overloads when a command timeout is not explicitly provided.
    /// </summary>
    public virtual TimeSpan DefaultCommandTimeout { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling exceptions thrown by event handlers
    /// invoked by this driver. Defaults to <see cref="TransportErrorBehavior.Ignore"/>, meaning that
    /// exceptions from event handlers will be caught and logged but will not cause the driver to stop
    /// processing messages from the transport.
    /// Exceptions from handlers registered with
    /// <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/> participate in this behavior
    /// when they are not already owned by task capture. If the caller captures handler tasks
    /// using <see cref="EventObserver{T}.WaitForCapturedTasksAsync"/>,
    /// <see cref="EventObserver{T}.WaitForCapturedTasksCompleteAsync"/>,
    /// or <see cref="EventObserver{T}.GetCapturedTasks"/>,
    /// those task exceptions remain owned by the caller rather than being surfaced again through the
    /// transport error pipeline.
    /// </summary>
    public virtual TransportErrorBehavior EventHandlerExceptionBehavior { get => this.transport.EventHandlerExceptionBehavior; set => this.transport.EventHandlerExceptionBehavior = value; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling exceptions when a protocol error is
    /// received from the remote end. Defaults to <see cref="TransportErrorBehavior.Ignore"/>, meaning
    /// that exceptions from protocol errors will be caught and logged but will not cause the driver to
    /// stop processing messages from the transport.
    /// </summary>
    public virtual TransportErrorBehavior ProtocolErrorBehavior { get => this.transport.ProtocolErrorBehavior; set => this.transport.ProtocolErrorBehavior = value; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling exceptions when an unknown message is
    /// encountered, such as valid JSON that does not match any protocol data structure. Defaults to
    /// <see cref="TransportErrorBehavior.Ignore"/>, meaning that exceptions from unknown messages will
    /// be caught and logged, but will not cause the driver to stop processing messages from the transport.
    /// </summary>
    public virtual TransportErrorBehavior UnknownMessageBehavior { get => this.transport.UnknownMessageBehavior; set => this.transport.UnknownMessageBehavior = value; }

    /// <summary>
    /// Gets or sets a value indicating the behavior for handling exceptions when an unexpected error is
    /// encountered, such as an error response received with no corresponding command. Defaults to
    /// <see cref="TransportErrorBehavior.Ignore"/>, meaning that exceptions from unexpected errors will
    /// be caught and logged but will not cause the driver to stop processing messages from the transport.
    /// </summary>
    public virtual TransportErrorBehavior UnexpectedErrorBehavior { get => this.transport.UnexpectedErrorBehavior; set => this.transport.UnexpectedErrorBehavior = value; }

    /// <summary>
    /// Gets the callback used to report late observer execution errors to the underlying transport.
    /// </summary>
    Func<EventObserverErrorInfo, Task> IEventObserverErrorReporter.EventObserverErrorReporter => this.ReportObservableEventObserverError;

    /// <summary>
    /// Gets a value indicating whether this driver is disposed.
    /// Use this property to ensure thread-safe operations for checking disposal state.
    /// </summary>
    protected bool IsDisposed => Interlocked.CompareExchange(ref this.isDisposedFlag, 0, 0) == 1;

    /// <summary>
    /// Asynchronously starts the communication with the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <param name="connectionString">The connection string used to connect to the remote end. Usually the URL to the WebSocket used to communicate with the remote end.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the driver has already been started.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to call this method after the driver is disposed.</exception>
    public virtual async Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        await this.transport.ConnectAsync(connectionString, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously stops the communication with the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="AggregateException">
    /// Thrown when <see cref="TransportErrorBehavior.Collect"/> is configured
    /// for any error category and one or more errors were collected during
    /// the session. The aggregated exceptions describe the collected errors.
    /// </exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.transport.DisconnectAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for a response.
    /// The result type is inferred from the command parameters.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="commandParameters">The object containing settings for the command, including parameters.</param>
    /// <param name="commandTimeout">The timeout to wait for the command to complete.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Defaults to <see cref="CancellationToken.None"/>, if unspecified.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiCommandException">Thrown if an error occurs during the execution of the command.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown if the command execution exceeds the specified timeout.</exception>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown if the connection is interrupted during command execution.</exception>
    /// <exception cref="WebDriverBiDiException">Thrown if the command is cancelled, returns a null value, or does not return a result of the correct object type.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to call this method after the driver is disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandParameters"/> is null.</exception>
    public virtual async Task<T> ExecuteCommandAsync<T>(CommandParameters<T> commandParameters, TimeSpan? commandTimeout = null, CancellationToken cancellationToken = default)
        where T : CommandResult
    {
        return await this.ExecuteCommandAsync<T>((CommandParameters)commandParameters, commandTimeout, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver BiDi protocol and waits for a response.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="commandParameters">The object containing settings for the command, including parameters.</param>
    /// <param name="commandTimeout">The timeout to wait for the command to complete.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Defaults to <see cref="CancellationToken.None"/>, if unspecified.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiCommandException">Thrown if an error occurs during the execution of the command.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown if the command execution exceeds the specified timeout.</exception>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown if the connection is interrupted during command execution.</exception>
    /// <exception cref="WebDriverBiDiException">Thrown if the command is cancelled, returns a null value, or does not return a result of the correct object type.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to call this method after the driver is disposed.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="commandParameters"/> is null.</exception>
    public virtual async Task<T> ExecuteCommandAsync<T>(CommandParameters commandParameters, TimeSpan? commandTimeout = null, CancellationToken cancellationToken = default)
        where T : CommandResult
    {
        this.ThrowIfDisposed();
        if (commandParameters is null)
        {
            throw new ArgumentNullException(nameof(commandParameters), $"Command parameters may not be null; must be a parameters object expecting a results of type {typeof(T)}");
        }

        commandTimeout ??= this.DefaultCommandTimeout;
        if (commandTimeout.Value < TimeSpan.Zero && commandTimeout.Value != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(commandTimeout), "Command timeout must be a non-negative TimeSpan value");
        }

        Command command = await this.transport.SendCommandAsync(commandParameters, cancellationToken).ConfigureAwait(false);
        try
        {
            bool commandCompleted = await command.WaitForCompletionAsync(commandTimeout.Value, cancellationToken).ConfigureAwait(false);
            if (!commandCompleted)
            {
                this.transport.CancelCommand(command);
                throw new WebDriverBiDiTimeoutException($"Timed out executing command {commandParameters.MethodName} after {commandTimeout.Value.TotalMilliseconds} milliseconds");
            }
        }
        catch (OperationCanceledException)
        {
            this.transport.CancelCommand(command);
            throw;
        }

        if (!command.TryGetResult(out CommandResult? result))
        {
            // Some code coverage tools do not recognize full coverage when using
            // ExceptionDispatchInfo.Throw() to throw the exception; They see a
            // closing brace as unreachable. To work around this, we can omit the
            // braces for the single-line if statement. However, this runs afoul
            // of the coding style rules for this project, which require braces
            // for all control blocks. Therefore, we disable those style rules
            // for this block only.
#pragma warning disable IDE0011, SA1503
            if (command.ThrownException is not null)
                ExceptionDispatchInfo.Capture(command.ThrownException).Throw();
#pragma warning restore IDE0011, SA1503

            if (command.IsCanceled)
            {
                throw new WebDriverBiDiException($"Command {commandParameters.MethodName} with id {command.CommandId} was canceled before a result was received");
            }
        }

        // This is purely defensive. The transport should never complete a command
        // with a null result, but if it does, that's an error condition that we want
        // to report explicitly rather than having it manifest as a null reference
        // exception later in the code.
        if (result is null)
        {
            throw new WebDriverBiDiException($"Result for command {commandParameters.MethodName} with id {command.CommandId} is unexpectedly null");
        }

        if (result.IsError)
        {
            if (result is not ErrorResult errorResponse)
            {
                throw new WebDriverBiDiException("Could not convert error response from transport for SendCommandAndWait to ErrorResult");
            }

            throw new WebDriverBiDiCommandException($"Received {errorResponse.ErrorCode} '{errorResponse.ErrorType}' error executing command {commandParameters.MethodName}: {errorResponse.ErrorMessage}", errorResponse);
        }

        if (result is not T convertedResult)
        {
            throw new WebDriverBiDiException($"Could not convert response from transport for SendCommandAndWait to {typeof(T)}");
        }

        return convertedResult;
    }

    /// <summary>
    /// Registers an event to be raised by the remote end of the WebDriver BiDi protocol.
    /// </summary>
    /// <typeparam name="T">The type of data that will be raised by the event.</typeparam>
    /// <param name="eventName">The name of the event to raise.</param>
    /// <param name="eventInvoker">The delegate taking a single parameter of type T used to invoke the event.</param>
    /// <exception cref="ArgumentException">Thrown when the specified event name is already registered with this driver, or when the event name is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the event invoker argument is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to call this method after the driver is disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to call this method after the driver has been started.</exception>
    public virtual void RegisterEvent<T>(string eventName, Func<EventInfo<T>, Task> eventInvoker)
    {
        this.ThrowIfDisposed();
        lock (this.eventRegistrationLock)
        {
            if (this.IsStarted)
            {
                throw new InvalidOperationException("Cannot register an event after the driver has started");
            }

            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentException("Event name may not be null or empty", nameof(eventName));
            }

            if (eventInvoker is null)
            {
                throw new ArgumentNullException(nameof(eventInvoker), "Event invoker may not be null");
            }

            if (!this.eventInvokers.TryAdd(eventName, new EventInvoker<T>(eventInvoker)))
            {
                throw new ArgumentException($"An event named '{eventName}' has already been registered.", nameof(eventName));
            }

            this.transport.RegisterEventMessage<T>(eventName);
        }
    }

    /// <summary>
    /// Registers a module for use with this driver.
    /// </summary>
    /// <param name="module">The module object.</param>
    /// <exception cref="ArgumentException">Thrown when attempting to register a module with a name that has already been registered.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the module argument is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to call this method after the driver is disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to call this method after the driver has been started.</exception>
    /// <remarks>
    /// <para>
    /// <strong>Critical timing restriction:</strong> Modules must be registered <em>before</em> calling
    /// <see cref="StartAsync(string, CancellationToken)"/>. Once the driver has started, module registration
    /// is locked to prevent race conditions with event handling.
    /// </para>
    /// <para>
    /// <strong>Thread safety:</strong> This method is thread-safe and can be safely called from multiple
    /// threads concurrently. An internal lock ensures that the check against <see cref="IsStarted"/> and
    /// the module addition to the registry are performed atomically, preventing race conditions during
    /// concurrent registration attempts or when registering near the time of calling <see cref="StartAsync(string, CancellationToken)"/>.
    /// </para>
    /// <para>
    /// This method is used for registering custom modules that extend the WebDriver BiDi protocol.
    /// All standard modules (Browser, BrowsingContext, Script, Network, etc.) are registered automatically
    /// during driver construction and do not need explicit registration.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
    /// driver.RegisterModule(new CustomModule(driver));  // Must be before StartAsync
    /// await driver.StartAsync(webSocketUrl);
    /// </code>
    /// </para>
    /// </remarks>
    public virtual void RegisterModule(Module module)
    {
        this.ThrowIfDisposed();
        lock (this.moduleRegistrationLock)
        {
            if (this.IsStarted)
            {
                throw new InvalidOperationException("Cannot register a module after the driver has started");
            }

            if (module is null)
            {
                throw new ArgumentNullException(nameof(module), "Module object may not be null");
            }

            if (!this.modules.TryAdd(module.ModuleName, module))
            {
                throw new ArgumentException($"A module with the name '{module.ModuleName}' has already been registered", nameof(module));
            }
        }
    }

    /// <summary>
    /// Gets a module from the set of registered modules for this driver.
    /// </summary>
    /// <typeparam name="T">A module object which is a subclass of <see cref="Module"/>.</typeparam>
    /// <param name="moduleName">The name of the module to return.</param>
    /// <returns>The protocol module object.</returns>
    /// <exception cref="ArgumentException">Thrown when the specified module name is not registered with this driver, or when the module name is null or empty.</exception>
    /// <exception cref="InvalidCastException">Thrown when the registered module object is not of the expected type.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to call this method after the driver is disposed.</exception>
    public virtual T GetModule<T>(string moduleName)
        where T : Module
    {
        this.ThrowIfDisposed();
        if (string.IsNullOrEmpty(moduleName))
        {
            throw new ArgumentException("Module name may not be null or empty", nameof(moduleName));
        }

        if (!this.modules.TryGetValue(moduleName, out Module? module))
        {
            throw new ArgumentException($"Module '{moduleName}' is not registered with this driver", nameof(moduleName));
        }

        if (module is not T)
        {
            throw new InvalidCastException($"Module '{moduleName}' is registered with this driver, but the module object is not of type {typeof(T)}");
        }

        return (T)module;
    }

    /// <summary>
    /// Registers an additional <see cref="IJsonTypeInfoResolver"/> for JSON serialization
    /// and deserialization. This allows custom types, such as those from user-defined modules,
    /// to be serialized in AOT scenarios where reflection-based serialization is unavailable.
    /// This method must be called before starting the driver.
    /// </summary>
    /// <param name="resolver">The type info resolver to add.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the resolver argument is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the driver has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the driver has already been started.</exception>
    public virtual async Task RegisterTypeInfoResolverAsync(IJsonTypeInfoResolver resolver, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver), "Type info resolver may not be null");
        }

        // The transport will do the registration, and throw the proper exception if the
        // transport is already connected (and thus the driver is started).
        await this.transport.RegisterTypeInfoResolverAsync(resolver, cancellationToken).ConfigureAwait(false);
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
        if (this.SetDisposed())
        {
            try
            {
                await this.StopAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this.LogAsync($"Unexpected exception during disposal: {ex.Message}", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            }

            await this.transportEventReceivedObserver.DisposeAsync().ConfigureAwait(false);
            await this.transportErrorReceivedObserver.DisposeAsync().ConfigureAwait(false);
            await this.transportUnknownMessageReceivedObserver.DisposeAsync().ConfigureAwait(false);
            await this.transportLogMessageObserver.DisposeAsync().ConfigureAwait(false);
            await this.transportEventHandlerErrorOccurredObserver.DisposeAsync().ConfigureAwait(false);
            await this.transport.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Marks this <see cref="BiDiDriver"/> as disposed. Use this method to ensure
    /// thread-safe operations for setting object being disposed.
    /// </summary>
    /// <returns><see langword="true"/> if the object was not already disposed before calling this method; otherwise, <see langword="false"/>.</returns>
    protected bool SetDisposed()
    {
        return Interlocked.Exchange(ref this.isDisposedFlag, 1) == 0;
    }

    /// <summary>
    /// Asynchronously raises a logging event at the specified log level.
    /// </summary>
    /// <param name="message">The log message to raise in the event.</param>
    /// <param name="logLevel">The <see cref="WebDriverBiDiLogLevel"/> at which to raise the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected async Task LogAsync(string message, WebDriverBiDiLogLevel logLevel)
    {
        await this.OnLogMessage.NotifyObserversAsync(new LogMessageEventArgs(message, logLevel, LoggerComponentName)).ConfigureAwait(false);
    }

    private void ThrowIfDisposed()
    {
        if (this.IsDisposed)
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }
    }

    private Task ReportObservableEventObserverError(EventObserverErrorInfo errorInfo)
    {
        return this.transport.ReportEventObserverErrorAsync(errorInfo);
    }

    private ObservableEvent<T> CreateObservableEvent<T>(string eventName)
        where T : WebDriverBiDiEventArgs
    {
        ObservableEvent<T> observableEvent = new(eventName);
        observableEvent.SetObserverErrorReporter(this.ReportObservableEventObserverError);
        return observableEvent;
    }

    private async Task OnTransportEventReceivedAsync(EventReceivedEventArgs e)
    {
        if (this.eventInvokers.TryGetValue(e.EventName, out EventInvoker? invoker))
        {
            await invoker.InvokeEventAsync(e.EventData, e.AdditionalData).ConfigureAwait(false);
        }

        await this.OnEventReceived.NotifyObserversAsync(e).ConfigureAwait(false);
    }

    private async Task OnTransportErrorEventReceivedAsync(ErrorReceivedEventArgs e)
    {
        await this.OnUnexpectedErrorReceived.NotifyObserversAsync(e).ConfigureAwait(false);
    }

    private async Task OnTransportUnknownMessageReceivedAsync(UnknownMessageReceivedEventArgs e)
    {
        await this.OnUnknownMessageReceived.NotifyObserversAsync(e).ConfigureAwait(false);
    }

    private async Task OnTransportEventHandlerErrorOccurredAsync(EventHandlerErrorOccurredEventArgs e)
    {
        await this.OnEventHandlerErrorOccurred.NotifyObserversAsync(e).ConfigureAwait(false);
    }

    private async Task OnTransportLogMessageAsync(LogMessageEventArgs e)
    {
        await this.OnLogMessage.NotifyObserversAsync(e).ConfigureAwait(false);
    }
}
