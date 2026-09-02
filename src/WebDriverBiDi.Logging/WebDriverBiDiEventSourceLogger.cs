// <copyright file="WebDriverBiDiEventSourceLogger.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Logging;

using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;

/// <summary>
/// EventListener that bridges WebDriverBiDi EventSource events to Microsoft.Extensions.Logging.ILogger.
/// This enables WebDriver BiDi diagnostic events to be captured by the standard .NET logging infrastructure.
/// </summary>
/// <remarks>
/// <para>
/// This class listens to events from <see cref="WebDriverBiDiEventSource"/> and forwards them to an
/// <see cref="ILogger"/> instance, mapping EventSource event levels to standard log levels.
/// </para>
/// <para>
/// Event properties are captured as structured log state, enabling rich logging scenarios with
/// structured logging providers like Application Insights, Serilog, etc.
/// </para>
/// <para>
/// <strong>Example usage:</strong>
/// </para>
/// <code>
/// services.AddLogging(builder => builder.AddWebDriverBiDi());
/// </code>
/// </remarks>
public sealed class WebDriverBiDiEventSourceLogger : EventListener
{
    // Declared nullable because the base EventListener constructor can deliver events (see
    // OnEventWritten) before this derived instance's constructor body assigns this field, during
    // which window a plain field read yields null.
    private readonly Lazy<ILogger>? logger;
    private readonly EventLevel minimumLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiEventSourceLogger"/> class.
    /// </summary>
    /// <param name="logger">The ILogger instance to forward events to.</param>
    /// <param name="minimumLevel">The minimum EventLevel to capture. Defaults to Informational.</param>
    public WebDriverBiDiEventSourceLogger(ILogger logger, EventLevel minimumLevel = EventLevel.Informational)
    {
        ILogger resolvedLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.logger = new Lazy<ILogger>(() => resolvedLogger);
        this.minimumLevel = minimumLevel;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiEventSourceLogger"/> class that
    /// resolves the target <see cref="ILogger"/> lazily on first use.
    /// </summary>
    /// <param name="logger">A factory that resolves the ILogger instance to forward events to.</param>
    /// <param name="minimumLevel">The minimum EventLevel to capture.</param>
    /// <remarks>
    /// The lazy factory lets the listener be constructed (and thereby subscribe to the EventSource)
    /// during logging-pipeline setup without resolving the <see cref="ILogger"/> from the very
    /// <see cref="ILoggerFactory"/> that is still being built. The factory is invoked once, on the
    /// first event forwarded, by which time the pipeline is available.
    /// </remarks>
    internal WebDriverBiDiEventSourceLogger(Lazy<ILogger> logger, EventLevel minimumLevel)
    {
        // This constructor is internal and is only ever called with a non-null factory (see
        // WebDriverBiDiLoggingExtensions.AddWebDriverBiDi), so no null guard is needed.
        this.logger = logger;
        this.minimumLevel = minimumLevel;
    }

    /// <summary>
    /// Called when an EventSource is created. Enables the WebDriverBiDi EventSource.
    /// </summary>
    /// <param name="eventSource">The EventSource that was created.</param>
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "WebDriverBiDi")
        {
            this.EnableEvents(eventSource, this.minimumLevel);
        }
    }

    /// <summary>
    /// Called when an event is written by an enabled EventSource.
    /// Forwards the event to the configured ILogger with structured properties.
    /// </summary>
    /// <param name="eventData">The event data containing event information and payload.</param>
    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventSource.Name != "WebDriverBiDi")
        {
            return;
        }

        // OnEventSourceCreated runs during the base EventListener constructor and can enable a
        // WebDriverBiDi EventSource that already exists when this listener is created, so an event may
        // be delivered here before the constructor body has assigned the logger field. An event in
        // that window has nowhere to go yet, so drop it rather than dereferencing the not-yet-assigned
        // field. This is the same base-constructor ordering caveat documented for minimumLevel below.
        Lazy<ILogger>? currentLogger = this.logger;
        if (currentLogger is null)
        {
            return;
        }

        // Enforce the configured minimum level here rather than relying solely on EnableEvents.
        // OnEventSourceCreated runs during the base EventListener constructor, before this instance's
        // minimum level is assigned, so a WebDriver BiDi EventSource that already exists when the
        // listener is created is enabled at the default level (everything). This authoritative check
        // keeps the configured level correct regardless of that ordering. EventLevel.LogAlways is the
        // "no filtering" level, so it captures every event; otherwise a higher EventLevel value is a
        // less severe event and is dropped when it exceeds the configured minimum.
        if (this.minimumLevel != EventLevel.LogAlways && eventData.Level > this.minimumLevel)
        {
            return;
        }

        LogLevel logLevel = MapEventLevel(eventData.Level);

        // Build structured log state with event properties
        Dictionary<string, object?> state = new()
        {
            ["EventId"] = eventData.EventId,
            ["EventName"] = eventData.EventName,
            ["EventSource"] = eventData.EventSource.Name,
        };

        // Add payload properties with their names. This construct
        // accesses payload names and payloads separately. If either
        // list is null or empty, no properties get added.
        ReadOnlyCollection<string>? payloadNames = eventData.PayloadNames;
        ReadOnlyCollection<object?>? payload = eventData.Payload;
        int payloadCount = Math.Min(payloadNames?.Count ?? 0, payload?.Count ?? 0);
        for (int i = 0; i < payloadCount; i++)
        {
            state[payloadNames![i]] = payload![i];
        }

        // Create EventId for the log entry
        EventId eventId = new(eventData.EventId, eventData.EventName);

        // Log with structured state
        currentLogger.Value.Log(logLevel, eventId, state, null, FormatMessage);
    }

    private static LogLevel MapEventLevel(EventLevel level)
    {
        return level switch
        {
            EventLevel.Critical => LogLevel.Critical,
            EventLevel.Error => LogLevel.Error,
            EventLevel.Warning => LogLevel.Warning,
            EventLevel.Informational => LogLevel.Information,
            EventLevel.Verbose => LogLevel.Debug,
            EventLevel.LogAlways => LogLevel.Information,
            _ => LogLevel.Trace,
        };
    }

    private static string FormatMessage(Dictionary<string, object?> state, Exception? exception)
    {
        // Try to format using the EventSource message template if available
        if (state.TryGetValue("EventName", out object? eventNameObj) && eventNameObj is string eventName)
        {
            // Build a simple message from the event name and key payload properties
            List<string> parts = new() { eventName };

            // Add key properties (skip metadata properties)
            foreach (KeyValuePair<string, object?> kvp in state)
            {
                if (kvp.Key is "EventId" or "EventName" or "EventSource")
                {
                    continue;
                }

                if (kvp.Value != null)
                {
                    parts.Add($"{kvp.Key}={kvp.Value}");
                }
            }

            return string.Join(", ", parts);
        }

        return "WebDriverBiDi event";
    }
}
