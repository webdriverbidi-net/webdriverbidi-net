// <copyright file="WebDriverBiDiEventSourceLogger.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Logging;

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
    private readonly ILogger logger;
    private readonly EventLevel minimumLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiEventSourceLogger"/> class.
    /// </summary>
    /// <param name="logger">The ILogger instance to forward events to.</param>
    /// <param name="minimumLevel">The minimum EventLevel to capture. Defaults to Informational.</param>
    public WebDriverBiDiEventSourceLogger(ILogger logger, EventLevel minimumLevel = EventLevel.Informational)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        LogLevel logLevel = MapEventLevel(eventData.Level);

        // Build structured log state with event properties
        Dictionary<string, object?> state = new()
        {
            ["EventId"] = eventData.EventId,
            ["EventName"] = eventData.EventName,
            ["EventSource"] = eventData.EventSource.Name,
        };

        // Add payload properties with their names
        if (eventData.PayloadNames != null && eventData.Payload != null)
        {
            for (int i = 0; i < eventData.PayloadNames.Count && i < eventData.Payload.Count; i++)
            {
                state[eventData.PayloadNames[i]] = eventData.Payload[i];
            }
        }

        // Create EventId for the log entry
        EventId eventId = new(eventData.EventId, eventData.EventName);

        // Log with structured state
        this.logger.Log(logLevel, eventId, state, null, FormatMessage);
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
