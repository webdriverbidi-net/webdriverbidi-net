// <copyright file="WebDriverBiDiEventSource.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Diagnostics.Tracing;
using WebDriverBiDi.Protocol;

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
///         // Payload[0] and [1] are the connection and session identifiers on every event except
///         // AsyncHandlerTaskCount; the values specific to the event follow them.
///         string connectionId = eventData.Payload?.Count > 0 ? eventData.Payload[0]?.ToString() ?? string.Empty : string.Empty;
///         string detail = eventData.Payload?.Count > 2 ? eventData.Payload[2]?.ToString() ?? string.Empty : string.Empty;
///         Console.WriteLine($"[{eventData.Level}] {connectionId} {eventData.EventName}: {detail}");
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
/// <strong>Example 3: EventPipe from another process</strong>
/// <code>
/// // Microsoft.Diagnostics.NETCore.Client, from the target process's ID:
/// DiagnosticsClient client = new(processId);
/// using EventPipeSession session = client.StartEventPipeSession(new EventPipeProvider("WebDriverBiDi", EventLevel.Informational));
/// // Read session.EventStream with TraceEvent...
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
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="url">The URL being connected to.</param>
    [Event(1, Level = EventLevel.Informational, Message = "[{0}/{1}] Opening connection to {2}")]
    public void ConnectionOpening(string connectionId, string sessionId, string url)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(1, connectionId, sessionId, url);
        }
    }

    /// <summary>
    /// Logs when a connection has been successfully opened.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="url">The URL that was connected to.</param>
    [Event(2, Level = EventLevel.Informational, Message = "[{0}/{1}] Connection opened to {2}")]
    public void ConnectionOpened(string connectionId, string sessionId, string url)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(2, connectionId, sessionId, url);
        }
    }

    /// <summary>
    /// Logs when a connection is being closed.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="reason">The reason for closing the connection.</param>
    [Event(3, Level = EventLevel.Informational, Message = "[{0}/{1}] Closing connection: {2}")]
    public void ConnectionClosing(string connectionId, string sessionId, string reason)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(3, connectionId, sessionId, reason);
        }
    }

    /// <summary>
    /// Logs when a connection has been closed.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    [Event(4, Level = EventLevel.Informational, Message = "[{0}/{1}] Connection closed")]
    public void ConnectionClosed(string connectionId, string sessionId)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(4, connectionId, sessionId);
        }
    }

    /// <summary>
    /// Logs when a connection error occurs.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="errorMessage">The error message.</param>
    [Event(5, Level = EventLevel.Error, Message = "[{0}/{1}] Connection error: {2}")]
    public void ConnectionError(string connectionId, string sessionId, string errorMessage)
    {
        if (this.IsEnabled(EventLevel.Error, EventKeywords.None))
        {
            this.WriteEvent(5, connectionId, sessionId, errorMessage);
        }
    }

    /// <summary>
    /// Logs when a command is being sent to the remote end.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    [NonEvent]
    public void CommandSending(string connectionId, string sessionId, long commandId, string method)
    {
        if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            this.CommandSending(connectionId, sessionId, commandId.ToString(), method);
        }
    }

    /// <summary>
    /// Logs when a command response is received from the remote end.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
    [NonEvent]
    public void CommandCompleted(string connectionId, string sessionId, long commandId, string method, long elapsedMilliseconds)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.CommandCompleted(connectionId, sessionId, commandId.ToString(), method, elapsedMilliseconds);
        }
    }

    /// <summary>
    /// Logs when a command times out waiting for a response.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="timeoutMilliseconds">The timeout duration in milliseconds.</param>
    [NonEvent]
    public void CommandTimeout(string connectionId, string sessionId, long commandId, string method, long timeoutMilliseconds)
    {
        if (this.IsEnabled(EventLevel.Warning, EventKeywords.None))
        {
            this.CommandTimeout(connectionId, sessionId, commandId.ToString(), method, timeoutMilliseconds);
        }
    }

    /// <summary>
    /// Logs when a command receives an error response from the remote end.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="errorCode">The protocol error code returned by the remote end.</param>
    /// <param name="errorType">The error type returned by the remote end.</param>
    /// <param name="errorMessage">The error message returned by the remote end.</param>
    [NonEvent]
    public void CommandError(string connectionId, string sessionId, long commandId, string method, ErrorCode errorCode, string errorType, string errorMessage)
    {
        if (this.IsEnabled(EventLevel.Error, EventKeywords.None))
        {
            this.CommandError(connectionId, sessionId, commandId.ToString(), method, errorCode.ToString(), errorType, errorMessage);
        }
    }

    /// <summary>
    /// Logs when an event is received from the remote end.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="eventMethod">The event method name.</param>
    [Event(10, Level = EventLevel.Verbose, Message = "[{0}/{1}] Event received: {2}")]
    public void EventReceived(string connectionId, string sessionId, string eventMethod)
    {
        if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            this.WriteEvent(10, connectionId, sessionId, eventMethod);
        }
    }

    // Event ID 11 has been retired.
    // Event ID 12 has been retired.

    /// <summary>
    /// Logs when an unknown message is received from the remote end.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="messageKind">The <see cref="IncomingMessageKind"/> of unknown message.</param>
    /// <param name="messageLength">The length of the message in bytes.</param>
    [NonEvent]
    public void UnknownMessageReceived(string connectionId, string sessionId, IncomingMessageKind messageKind, int messageLength)
    {
        if (this.IsEnabled(EventLevel.Warning, EventKeywords.None))
        {
            string messageKindDescription = messageKind switch
            {
                IncomingMessageKind.CommandResponse => "success",
                IncomingMessageKind.ErrorResponse => "error",
                IncomingMessageKind.Event => "event",
                _ => "unknown",
            };
            this.UnknownMessageReceived(connectionId, sessionId, messageKindDescription, messageLength);
        }
    }

    /// <summary>
    /// Logs when a protocol error occurs during message processing.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="messageSnippet">A snippet of the problematic message (truncated for safety).</param>
    [Event(14, Level = EventLevel.Error, Message = "[{0}/{1}] Protocol error: {2} (message: {3})")]
    public void ProtocolError(string connectionId, string sessionId, string errorMessage, string messageSnippet)
    {
        if (this.IsEnabled(EventLevel.Error, EventKeywords.None))
        {
            this.WriteEvent(14, [connectionId, sessionId, errorMessage, messageSnippet]);
        }
    }

    /// <summary>
    /// Logs when an error occurs in an event handler.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="eventMethod">The event method name.</param>
    /// <param name="errorMessage">The error message from the exception.</param>
    [Event(15, Level = EventLevel.Warning, Message = "[{0}/{1}] Event handler error for {2}: {3}")]
    public void EventHandlerError(string connectionId, string sessionId, string eventMethod, string errorMessage)
    {
        if (this.IsEnabled(EventLevel.Warning, EventKeywords.None))
        {
            this.WriteEvent(15, [connectionId, sessionId, eventMethod, errorMessage]);
        }
    }

    /// <summary>
    /// Logs the current count of pending commands waiting for responses.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="pendingCount">The number of pending commands.</param>
    [Event(16, Level = EventLevel.Verbose, Message = "[{0}/{1}] Pending commands: {2}")]
    public void PendingCommandCount(string connectionId, string sessionId, int pendingCount)
    {
        if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            this.WriteEvent(16, connectionId, sessionId, pendingCount);
        }
    }

    /// <summary>
    /// Logs when the transport starts processing messages.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    [Event(17, Level = EventLevel.Informational, Message = "[{0}/{1}] Transport started")]
    public void TransportStarted(string connectionId, string sessionId)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(17, connectionId, sessionId);
        }
    }

    /// <summary>
    /// Logs when the transport stops processing messages.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="reason">The reason for stopping.</param>
    [Event(18, Level = EventLevel.Informational, Message = "[{0}/{1}] Transport stopped: {2}")]
    public void TransportStopped(string connectionId, string sessionId, string reason)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(18, connectionId, sessionId, reason);
        }
    }

    /// <summary>
    /// Logs when a custom module is registered with the driver.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="moduleName">The name of the custom module.</param>
    [Event(19, Level = EventLevel.Informational, Message = "[{0}/{1}] Custom module registered: {2}")]
    public void CustomModuleRegistered(string connectionId, string sessionId, string moduleName)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(19, connectionId, sessionId, moduleName);
        }
    }

    /// <summary>
    /// Logs when a custom event type is registered with the driver.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="eventName">The name of the custom event.</param>
    /// <param name="eventType">The .NET type handling the event.</param>
    [Event(20, Level = EventLevel.Informational, Message = "[{0}/{1}] Custom event registered: {2} -> {3}")]
    public void CustomEventRegistered(string connectionId, string sessionId, string eventName, string eventType)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.WriteEvent(20, [connectionId, sessionId, eventName, eventType]);
        }
    }

    /// <summary>
    /// Logs detailed message processing statistics.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="messagesSent">Number of commands sent during the session.</param>
    /// <param name="messagesReceived">Number of command responses received during the session.</param>
    /// <param name="eventsReceived">Number of events received during the session.</param>
    /// <param name="errorsReceived">Number of error responses received during the session.</param>
    /// <remarks>
    /// Raised when a session of the transport ends, by disconnection or by loss of the connection, with the
    /// counts for that session alone. A message counts only once it has been processed, so a message still
    /// waiting to be processed when the snapshot is taken is not included.
    /// </remarks>
    [Event(21, Level = EventLevel.Verbose, Message = "[{0}/{1}] Stats: sent={2}, received={3}, events={4}, errors={5}")]
    public void MessageStatistics(string connectionId, string sessionId, long messagesSent, long messagesReceived, long eventsReceived, long errorsReceived)
    {
        if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            this.WriteEvent(21, [connectionId, sessionId, messagesSent, messagesReceived, eventsReceived, errorsReceived]);
        }
    }

    /// <summary>
    /// Logs when a command fails before it can be successfully transmitted.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="failureType">The .NET exception type describing the send failure.</param>
    /// <param name="failureMessage">The failure message.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds before the send failed.</param>
    [NonEvent]
    public void CommandSendFailed(string connectionId, string sessionId, long commandId, string method, string failureType, string failureMessage, long elapsedMilliseconds)
    {
        if (this.IsEnabled(EventLevel.Warning, EventKeywords.None))
        {
            this.CommandSendFailed(connectionId, sessionId, commandId.ToString(), method, failureType, failureMessage, elapsedMilliseconds);
        }
    }

    /// <summary>
    /// Logs the current count of in-flight asynchronous event handler tasks.
    /// </summary>
    /// <remarks>
    /// This counter tracks handlers registered with
    /// <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/> whose returned
    /// <see cref="Task"/> has not yet completed. A persistently growing value indicates that
    /// asynchronous handlers are accumulating faster than they complete; this may precede
    /// memory pressure and should prompt investigation of handler duration or event rate.
    /// The counter is process-global across all <see cref="BiDiDriver"/> instances, and so carries no
    /// connection or session identifier, unlike every other event on this source.
    /// </remarks>
    /// <param name="inFlightCount">The number of in-flight asynchronous handler tasks.</param>
    [Event(23, Level = EventLevel.Verbose, Message = "In-flight async handler tasks: {0}")]
    public void AsyncHandlerTaskCount(int inFlightCount)
    {
        if (this.IsEnabled(EventLevel.Verbose, EventKeywords.None))
        {
            this.WriteEvent(23, inFlightCount);
        }
    }

    /// <summary>
    /// Raises the event indicating that a response was received for a command the local end had
    /// already stopped waiting for (because it timed out, was canceled, or the connection was closed),
    /// and that the response was discarded.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The ID of the canceled command.</param>
    /// <param name="method">The protocol method name of the canceled command.</param>
    /// <param name="reason">The reason the command was canceled.</param>
    /// <param name="millisecondsSinceCancellation">The time, in milliseconds, between the cancellation and the arrival of the response.</param>
    [NonEvent]
    public void CanceledCommandResponseDiscarded(string connectionId, string sessionId, long commandId, string method, CommandCancellationReason reason, long millisecondsSinceCancellation)
    {
        if (this.IsEnabled(EventLevel.Informational, EventKeywords.None))
        {
            this.CanceledCommandResponseDiscarded(connectionId, sessionId, commandId.ToString(), method, reason.ToString(), millisecondsSinceCancellation);
        }
    }

    /// <summary>
    /// Logs when a command is being sent to the remote end.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    [Event(6, Level = EventLevel.Verbose, Message = "[{0}/{1}] Sending command {2}: {3}")]
    private void CommandSending(string connectionId, string sessionId, string commandId, string method)
    {
        this.WriteEvent(6, [connectionId, sessionId, commandId, method]);
    }

    /// <summary>
    /// Logs when a command response is received from the remote end.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
    [Event(7, Level = EventLevel.Informational, Message = "[{0}/{1}] Command {2} ({3}) completed in {4}ms")]
    private void CommandCompleted(string connectionId, string sessionId, string commandId, string method, long elapsedMilliseconds)
    {
        this.WriteEvent(7, [connectionId, sessionId, commandId, method, elapsedMilliseconds]);
    }

    /// <summary>
    /// Logs when a command times out waiting for a response.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="timeoutMilliseconds">The timeout duration in milliseconds.</param>
    [Event(8, Level = EventLevel.Warning, Message = "[{0}/{1}] Command {2} ({3}) timed out after {4}ms")]
    private void CommandTimeout(string connectionId, string sessionId, string commandId, string method, long timeoutMilliseconds)
    {
        this.WriteEvent(8, [connectionId, sessionId, commandId, method, timeoutMilliseconds]);
    }

    /// <summary>
    /// Logs when a command receives an error response from the remote end.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="errorCode">The protocol error code returned by the remote end.</param>
    /// <param name="errorType">The error type returned by the remote end.</param>
    /// <param name="errorMessage">The error message returned by the remote end.</param>
    [Event(9, Level = EventLevel.Error, Message = "[{0}/{1}] Command {2} ({3}) failed: {4} ({5}) - {6}")]
    private void CommandError(string connectionId, string sessionId, string commandId, string method, string errorCode, string errorType, string errorMessage)
    {
        this.WriteEvent(9, [connectionId, sessionId, commandId, method, errorCode, errorType, errorMessage]);
    }

    /// <summary>
    /// Logs when an unknown message is received from the remote end.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="messageType">The type of unknown message.</param>
    /// <param name="messageLength">The length of the message in bytes.</param>
    [Event(13, Level = EventLevel.Warning, Message = "[{0}/{1}] Unknown message received: type={2}, length={3}")]
    private void UnknownMessageReceived(string connectionId, string sessionId, string messageType, int messageLength)
    {
        this.WriteEvent(13, [connectionId, sessionId, messageType, messageLength]);
    }

    /// <summary>
    /// Logs when a command fails before it can be successfully transmitted.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="failureType">The .NET exception type describing the send failure.</param>
    /// <param name="failureMessage">The failure message.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds before the send failed.</param>
    [Event(22, Level = EventLevel.Warning, Message = "[{0}/{1}] Command {2} ({3}) failed before transmission: {4} - {5} after {6}ms")]
    private void CommandSendFailed(string connectionId, string sessionId, string commandId, string method, string failureType, string failureMessage, long elapsedMilliseconds)
    {
        this.WriteEvent(22, [connectionId, sessionId, commandId, method, failureType, failureMessage, elapsedMilliseconds]);
    }

    /// <summary>
    /// Logs when the response to a canceled command is discarded.
    /// </summary>
    /// <param name="connectionId">The unique identifier for the connection the event belongs to.</param>
    /// <param name="sessionId">The unique identifier for the session the event belongs to, or an empty string when no session is in progress.</param>
    /// <param name="commandId">The unique identifier for the command.</param>
    /// <param name="method">The command method name.</param>
    /// <param name="reason">The reason the command was canceled.</param>
    /// <param name="millisecondsSinceCancellation">The time between the cancellation and the discarded response.</param>
    [Event(24, Level = EventLevel.Informational, Message = "[{0}/{1}] Response for command {2} ({3}) discarded; the command was canceled ({4}) {5}ms earlier")]
    private void CanceledCommandResponseDiscarded(string connectionId, string sessionId, string commandId, string method, string reason, long millisecondsSinceCancellation)
    {
        this.WriteEvent(24, [connectionId, sessionId, commandId, method, reason, millisecondsSinceCancellation]);
    }
}
