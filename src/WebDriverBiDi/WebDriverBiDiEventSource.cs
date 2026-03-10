// <copyright file="WebDriverBiDiEventSource.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Diagnostics.Tracing;

/// <summary>
/// EventSource for WebDriver BiDi protocol instrumentation.
/// This EventSource provides structured diagnostic events for monitoring,
/// tracing, and troubleshooting WebDriver BiDi operations.
/// </summary>
/// <remarks>
/// <para>
/// This EventSource can be consumed by:
/// <list type="bullet">
/// <item><description>EventListener (custom in-process listeners)</description></item>
/// <item><description>ETW (Event Tracing for Windows)</description></item>
/// <item><description>EventPipe (cross-platform event collection)</description></item>
/// <item><description>dotnet-trace (CLI diagnostics tool)</description></item>
/// <item><description>OpenTelemetry (via EventSource integration)</description></item>
/// <item><description>Application Insights, Dynatrace, etc. (via bridges)</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Usage Examples:</strong>
/// </para>
/// <para>
/// <strong>Example 1: Custom EventListener</strong>
/// <code>
/// public class WebDriverBiDiEventListener : EventListener
/// {
///     protected override void OnEventSourceCreated(EventSource source)
///     {
///         if (source.Name == "WebDriverBiDi")
///         {
///             EnableEvents(source, EventLevel.Informational);
///         }
///     }
///
///     protected override void OnEventWritten(EventWrittenEventArgs eventData)
///     {
///         Console.WriteLine($"[{eventData.Level}] {eventData.EventName}: {eventData.Payload?[0]}");
///     }
/// }
/// </code>
/// </para>
/// <para>
/// <strong>Example 2: dotnet-trace</strong>
/// <code>
/// dotnet-trace collect --providers WebDriverBiDi --process-id &lt;pid&gt;
/// </code>
/// </para>
/// <para>
/// <strong>Example 3: EventPipe in code</strong>
/// <code>
/// using var session = EventPipeSystem.CreateSession(new EventPipeProvider("WebDriverBiDi"));
/// // Collect events...
/// </code>
/// </para>
/// </remarks>
[EventSource(Name = "WebDriverBiDi")]
public sealed class WebDriverBiDiEventSource : EventSource
{
    /// <summary>
    /// The singleton instance of the WebDriverBiDiEventSource.
    /// </summary>
    public static readonly WebDriverBiDiEventSource RaiseEvent = new();

    private WebDriverBiDiEventSource()
        : base(EventSourceSettings.EtwSelfDescribingEventFormat)
    {
    }

    /// <summary>
    /// Logs when a connection is being opened to the remote end.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection.</param>
    /// <param name="url">The URL being connected to.</param>
    [Event(1, Level = EventLevel.Informational, Message = "Opening connection {0} to {1}")]
    public void ConnectionOpening(string connectionId, string url)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(1, connectionId, url);
        }
    }

    /// <summary>
    /// Logs when a connection has been successfully opened.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection.</param>
    /// <param name="url">The URL that was connected to.</param>
    [Event(2, Level = EventLevel.Informational, Message = "Connection {0} opened to {1}")]
    public void ConnectionOpened(string connectionId, string url)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(2, connectionId, url);
        }
    }

    /// <summary>
    /// Logs when a connection is being closed.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection.</param>
    /// <param name="reason">The reason for closing the connection.</param>
    [Event(3, Level = EventLevel.Informational, Message = "Closing connection {0}: {1}")]
    public void ConnectionClosing(string connectionId, string reason)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(3, connectionId, reason);
        }
    }

    /// <summary>
    /// Logs when a connection has been closed.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection.</param>
    [Event(4, Level = EventLevel.Informational, Message = "Connection {0} closed")]
    public void ConnectionClosed(string connectionId)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(4, connectionId);
        }
    }

    /// <summary>
    /// Logs when a connection error occurs.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection.</param>
    /// <param name="errorMessage">The error message.</param>
    [Event(5, Level = EventLevel.Error, Message = "Connection {0} error: {1}")]
    public void ConnectionError(string connectionId, string errorMessage)
    {
        if (this.IsEnabled(EventLevel.Error, EventKeywords.None))
        {
            this.WriteEvent(5, connectionId, errorMessage);
        }
    }

    /// <summary>
    /// Logs when a command is being sent to the remote end.
    /// </summary>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    [Event(6, Level = EventLevel.Verbose, Message = "Sending command {0}: {1}")]
    public void CommandSending(string commandId, string method)
    {
        if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            this.WriteEvent(6, commandId, method);
        }
    }

    /// <summary>
    /// Logs when a command response is received from the remote end.
    /// </summary>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
    [Event(7, Level = EventLevel.Informational, Message = "Command {0} ({1}) completed in {2}ms")]
    public void CommandCompleted(string commandId, string method, long elapsedMilliseconds)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(7, commandId, method, elapsedMilliseconds);
        }
    }

    /// <summary>
    /// Logs when a command times out waiting for a response.
    /// </summary>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="timeoutMilliseconds">The timeout duration in milliseconds.</param>
    [Event(8, Level = EventLevel.Warning, Message = "Command {0} ({1}) timed out after {2}ms")]
    public void CommandTimeout(string commandId, string method, long timeoutMilliseconds)
    {
        if (this.IsEnabled(EventLevel.Warning, EventKeywords.None))
        {
            this.WriteEvent(8, commandId, method, timeoutMilliseconds);
        }
    }

    /// <summary>
    /// Logs when a command receives an error response from the remote end.
    /// </summary>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="errorType">The error type returned by the remote end.</param>
    /// <param name="errorMessage">The error message returned by the remote end.</param>
    [Event(9, Level = EventLevel.Error, Message = "Command {0} ({1}) failed: {2} - {3}")]
    public void CommandError(string commandId, string method, string errorType, string errorMessage)
    {
        if (this.IsEnabled(EventLevel.Error, EventKeywords.None))
        {
            this.WriteEvent(9, commandId, method, errorType, errorMessage);
        }
    }

    /// <summary>
    /// Logs when an event is received from the remote end.
    /// </summary>
    /// <param name="eventMethod">The event method name.</param>
    [Event(10, Level = EventLevel.Verbose, Message = "Event received: {0}")]
    public void EventReceived(string eventMethod)
    {
        if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            this.WriteEvent(10, eventMethod);
        }
    }

    /// <summary>
    /// Logs when event subscription is requested.
    /// </summary>
    /// <param name="eventNames">Comma-separated list of event names being subscribed to.</param>
    /// <param name="contextCount">The number of contexts the subscription applies to (0 for global).</param>
    [Event(11, Level = EventLevel.Informational, Message = "Subscribing to events: {0} (contexts: {1})")]
    public void EventSubscribing(string eventNames, int contextCount)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(11, eventNames, contextCount);
        }
    }

    /// <summary>
    /// Logs when event unsubscription is requested.
    /// </summary>
    /// <param name="eventNames">Comma-separated list of event names being unsubscribed from.</param>
    /// <param name="contextCount">The number of contexts the unsubscription applies to (0 for global).</param>
    [Event(12, Level = EventLevel.Informational, Message = "Unsubscribing from events: {0} (contexts: {1})")]
    public void EventUnsubscribing(string eventNames, int contextCount)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(12, eventNames, contextCount);
        }
    }

    /// <summary>
    /// Logs when an unknown message is received from the remote end.
    /// </summary>
    /// <param name="messageType">The type of unknown message.</param>
    /// <param name="messageLength">The length of the message in bytes.</param>
    [Event(13, Level = EventLevel.Warning, Message = "Unknown message received: type={0}, length={1}")]
    public void UnknownMessageReceived(string messageType, int messageLength)
    {
        if (this.IsEnabled(EventLevel.Warning, EventKeywords.None))
        {
            this.WriteEvent(13, messageType, messageLength);
        }
    }

    /// <summary>
    /// Logs when a protocol error occurs during message processing.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="messageSnippet">A snippet of the problematic message (truncated for safety).</param>
    [Event(14, Level = EventLevel.Error, Message = "Protocol error: {0} (message: {1})")]
    public void ProtocolError(string errorMessage, string messageSnippet)
    {
        if (this.IsEnabled(EventLevel.Error, EventKeywords.None))
        {
            this.WriteEvent(14, errorMessage, messageSnippet);
        }
    }

    /// <summary>
    /// Logs when an error occurs in an event handler.
    /// </summary>
    /// <param name="eventMethod">The event method name.</param>
    /// <param name="errorMessage">The error message from the exception.</param>
    [Event(15, Level = EventLevel.Warning, Message = "Event handler error for {0}: {1}")]
    public void EventHandlerError(string eventMethod, string errorMessage)
    {
        if (this.IsEnabled(EventLevel.Warning, EventKeywords.None))
        {
            this.WriteEvent(15, eventMethod, errorMessage);
        }
    }

    /// <summary>
    /// Logs the current count of pending commands waiting for responses.
    /// </summary>
    /// <param name="pendingCount">The number of pending commands.</param>
    [Event(16, Level = EventLevel.Verbose, Message = "Pending commands: {0}")]
    public void PendingCommandCount(int pendingCount)
    {
        if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            this.WriteEvent(16, pendingCount);
        }
    }

    /// <summary>
    /// Logs when the transport starts processing messages.
    /// </summary>
    [Event(17, Level = EventLevel.Informational, Message = "Transport started")]
    public void TransportStarted()
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(17);
        }
    }

    /// <summary>
    /// Logs when the transport stops processing messages.
    /// </summary>
    /// <param name="reason">The reason for stopping.</param>
    [Event(18, Level = EventLevel.Informational, Message = "Transport stopped: {0}")]
    public void TransportStopped(string reason)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(18, reason);
        }
    }

    /// <summary>
    /// Logs when a custom module is registered with the driver.
    /// </summary>
    /// <param name="moduleName">The name of the custom module.</param>
    [Event(19, Level = EventLevel.Informational, Message = "Custom module registered: {0}")]
    public void CustomModuleRegistered(string moduleName)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(19, moduleName);
        }
    }

    /// <summary>
    /// Logs when a custom event type is registered with the driver.
    /// </summary>
    /// <param name="eventName">The name of the custom event.</param>
    /// <param name="eventType">The .NET type handling the event.</param>
    [Event(20, Level = EventLevel.Informational, Message = "Custom event registered: {0} -> {1}")]
    public void CustomEventRegistered(string eventName, string eventType)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(20, eventName, eventType);
        }
    }

    /// <summary>
    /// Logs detailed message processing statistics.
    /// </summary>
    /// <param name="messagesSent">Total number of messages sent.</param>
    /// <param name="messagesReceived">Total number of messages received.</param>
    /// <param name="eventsReceived">Total number of events received.</param>
    /// <param name="errorsReceived">Total number of errors received.</param>
    [Event(21, Level = EventLevel.Verbose, Message = "Stats: sent={0}, received={1}, events={2}, errors={3}")]
    public void MessageStatistics(long messagesSent, long messagesReceived, long eventsReceived, long errorsReceived)
    {
        if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            this.WriteEvent(21, messagesSent, messagesReceived, eventsReceived, errorsReceived);
        }
    }

    /// <summary>
    /// Logs when a command fails before it can be successfully transmitted.
    /// </summary>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="failureType">The .NET exception type describing the send failure.</param>
    /// <param name="failureMessage">The failure message.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds before the send failed.</param>
    [Event(22, Level = EventLevel.Warning, Message = "Command {0} ({1}) failed before transmission: {2} - {3} after {4}ms")]
    public void CommandSendFailed(string commandId, string method, string failureType, string failureMessage, long elapsedMilliseconds)
    {
        if (this.IsEnabled(EventLevel.Warning, EventKeywords.None))
        {
            this.WriteEvent(22, [commandId, method, failureType, failureMessage, elapsedMilliseconds]);
        }
    }
}
