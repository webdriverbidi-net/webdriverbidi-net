// <copyright file="WebDriverBiDiEventListener.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Demo;

using System.Diagnostics.Tracing;

/// <summary>
/// Example EventListener implementation that demonstrates how to consume
/// WebDriverBiDi EventSource diagnostic events.
/// </summary>
/// <remarks>
/// This is a simple example that logs events to the console. In production scenarios,
/// you would typically:
/// <list type="bullet">
/// <item><description>Forward events to a structured logging framework (Serilog, NLog, etc.)</description></item>
/// <item><description>Send events to Application Insights, Dynatrace, or other APM tools</description></item>
/// <item><description>Use OpenTelemetry ActivityListener for distributed tracing</description></item>
/// <item><description>Aggregate events for metrics and alerting</description></item>
/// </list>
/// </remarks>
public class WebDriverBiDiEventListener : EventListener
{
    private readonly EventLevel minimumLevel;
    private readonly bool showVerbose;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebDriverBiDiEventListener"/> class.
    /// </summary>
    /// <param name="minimumLevel">The minimum event level to listen for. Defaults to Informational.</param>
    /// <param name="showVerbose">Whether to show verbose output including event payloads. Defaults to false.</param>
    public WebDriverBiDiEventListener(EventLevel minimumLevel = EventLevel.Informational, bool showVerbose = false)
    {
        this.minimumLevel = minimumLevel;
        this.showVerbose = showVerbose;
    }

    /// <summary>
    /// Called when an EventSource is created. This is where we enable the WebDriverBiDi EventSource.
    /// </summary>
    /// <param name="eventSource">The EventSource that was created.</param>
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        // Enable the WebDriverBiDi EventSource when it's created
        if (eventSource.Name == "WebDriverBiDi")
        {
            this.EnableEvents(eventSource, this.minimumLevel);
        }
    }

    /// <summary>
    /// Called when an event is written by an enabled EventSource.
    /// </summary>
    /// <param name="eventData">The event data containing event information and payload.</param>
    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventSource.Name != "WebDriverBiDi")
        {
            return;
        }

        // Format the timestamp
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

        // Format the event level with color
        string levelIndicator = this.GetLevelIndicator(eventData.Level);

        // Build the message
        string message = this.FormatEventMessage(eventData);

        // Write to console with level-based formatting
        Console.WriteLine($"[{timestamp}] {levelIndicator} {message}");

        // Optionally show detailed payload information
        if (this.showVerbose && eventData.Payload != null && eventData.Payload.Count > 0)
        {
            Console.WriteLine($"    Payload: {string.Join(", ", eventData.Payload.Select((p, i) => $"{eventData.PayloadNames?[i] ?? $"arg{i}"}={p}"))}");
        }
    }

    private string GetLevelIndicator(EventLevel level)
    {
        return level switch
        {
            EventLevel.Critical => "[CRIT]",
            EventLevel.Error => "[ERROR]",
            EventLevel.Warning => "[WARN]",
            EventLevel.Informational => "[INFO]",
            EventLevel.Verbose => "[VERB]",
            _ => "[????]",
        };
    }

    private string FormatEventMessage(EventWrittenEventArgs eventData)
    {
        // If the event has a formatted message, use it
        if (!string.IsNullOrEmpty(eventData.Message) && eventData.Payload != null)
        {
            try
            {
                return string.Format(eventData.Message, eventData.Payload.ToArray());
            }
            catch
            {
                // Fall back to manual formatting if string.Format fails
            }
        }

        // Otherwise, build a message from the event name and payload
        if (eventData.Payload != null && eventData.Payload.Count > 0)
        {
            string payloadStr = string.Join(", ", eventData.Payload.Select((p, i) =>
            {
                string name = eventData.PayloadNames?[i] ?? $"arg{i}";
                return $"{name}={p}";
            }));
            return $"{eventData.EventName}: {payloadStr}";
        }

        return eventData.EventName ?? "[UnnamedEvent]";
    }
}
