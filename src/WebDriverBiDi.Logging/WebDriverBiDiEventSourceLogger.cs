// <copyright file="WebDriverBiDiEventSourceLogger.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Logging;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Text;
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
/// The log message is rendered from the message template the <see cref="WebDriverBiDiEventSource"/> declares
/// for the event. The state also carries that template under the <c>{OriginalFormat}</c> key, with each
/// positional hole renamed after the payload property it refers to, so template-aware providers see a stable
/// template whose holes match the structured properties. An event without a template is described by its
/// name and payload instead.
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
    // The state key under which the logging abstractions carry a log entry's message template.
    private const string OriginalFormatKey = "{OriginalFormat}";

    // The named form of each event's message template, keyed by event ID, so a template is rewritten once
    // rather than on every event. Assigned by its initializer, which runs before the base constructor, so it
    // is never null.
    private readonly ConcurrentDictionary<int, string> namedMessageTemplates = new();

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
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The argument is checked in the constructor initializer rather than in a body, because the base
    /// <see cref="EventListener"/> constructor subscribes this instance to the EventSource before any
    /// body runs. Throwing after that point would leave the subscription in place on an object the
    /// caller never receives and so can never dispose, and <see cref="EventListener"/> keeps every
    /// listener in a static list, so the orphan would go on holding the source enabled at
    /// <see cref="EventLevel.LogAlways"/> for the life of the process. Arguments to a constructor
    /// initializer are evaluated before the base constructor runs, so the exception is thrown before
    /// there is a listener to orphan.
    /// </remarks>
    public WebDriverBiDiEventSourceLogger(ILogger logger, EventLevel minimumLevel = EventLevel.Informational)
        : this(CreateResolvedLoggerFactory(logger), minimumLevel)
    {
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
        this.EnableConfiguredEvents();
    }

    /// <summary>
    /// Called when an EventSource is created. Enables the WebDriverBiDi EventSource.
    /// </summary>
    /// <param name="eventSource">The EventSource that was created.</param>
    /// <remarks>
    /// For a source created after this listener exists, the configured minimum level is already assigned and
    /// this subscribes at it. For a source that already existed, the base <see cref="EventListener"/> constructor
    /// calls this before the derived constructor body runs, so the level read here is still the default; the
    /// constructor re-subscribes at the configured level once it is known.
    /// </remarks>
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

        // Consult the target logger's own filtering before doing any per-event work. The listener
        // subscribes to the EventSource at the configured EventLevel, but the ILogger may still have
        // this category filtered off; checking IsEnabled here avoids allocating and boxing the
        // structured state for an event that would only be discarded. Resolving the logger is required
        // to make this check and is a one-time cost cached by the Lazy.
        ILogger target = currentLogger.Value;
        if (!target.IsEnabled(logLevel))
        {
            return;
        }

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

        // The EventSource declares each event's message as a template with positional holes ({0}).
        // Template-aware providers bind the holes of the template they are given to state properties by
        // name, so the template carried in the state has each hole renamed after its payload property.
        string? messageTemplate = this.GetNamedMessageTemplate(eventData, payloadNames, payloadCount);
        if (messageTemplate is not null)
        {
            state[OriginalFormatKey] = messageTemplate;
        }

        // Create EventId for the log entry
        EventId eventId = new(eventData.EventId, eventData.EventName);

        // Log with structured state
        target.Log(logLevel, eventId, state, null, FormatMessage);
    }

    /// <summary>
    /// Wraps an already-resolved logger in the factory the listener holds, after checking it is not null.
    /// </summary>
    /// <param name="logger">The ILogger instance to forward events to.</param>
    /// <returns>A factory returning that logger.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
    private static Lazy<ILogger> CreateResolvedLoggerFactory(ILogger logger)
    {
        ILogger resolvedLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        return new Lazy<ILogger>(() => resolvedLogger);
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
        // Render the event's message template when the event declares one.
        if (state.TryGetValue(OriginalFormatKey, out object? templateObj) && templateObj is string template)
        {
            return RenderMessageTemplate(template, state);
        }

        // An event without a template, such as a self-describing event written with EventSource.Write, is
        // described by its name and payload instead.
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

    /// <summary>
    /// Rewrites an EventSource message template's positional holes as named holes.
    /// </summary>
    /// <param name="template">The message template, whose holes are positional (<c>{0}</c>, <c>{1}</c>, and so on).</param>
    /// <param name="payloadNames">The names of the event's payload properties, in payload order.</param>
    /// <param name="payloadCount">The number of payload properties.</param>
    /// <returns>The template with each positional hole replaced by the name of the payload property at that position.</returns>
    /// <remarks>
    /// The holes are replaced in the template itself, never in a rendered message, so a payload value can
    /// never be mistaken for a hole. The library's templates use only plain positional holes, with no
    /// alignment, format specifier or escaped brace.
    /// </remarks>
    private static string ConvertToNamedTemplate(string template, ReadOnlyCollection<string>? payloadNames, int payloadCount)
    {
        string namedTemplate = template;
        for (int i = 0; i < payloadCount; i++)
        {
            namedTemplate = namedTemplate.Replace("{" + i.ToString(CultureInfo.InvariantCulture) + "}", "{" + payloadNames![i] + "}");
        }

        return namedTemplate;
    }

    /// <summary>
    /// Renders a message template with named holes from the values in the log state.
    /// </summary>
    /// <param name="template">The message template, whose holes name entries in <paramref name="state"/>.</param>
    /// <param name="state">The log state holding the values.</param>
    /// <returns>The rendered message.</returns>
    /// <remarks>
    /// The template is scanned once, so a value that itself contains braces is copied as it is rather than
    /// being read as a hole. A hole that names no state entry, and an unterminated brace, are copied literally.
    /// </remarks>
    private static string RenderMessageTemplate(string template, Dictionary<string, object?> state)
    {
        StringBuilder message = new(template.Length);
        int position = 0;
        while (position < template.Length)
        {
            int holeStart = template.IndexOf('{', position);
            int holeEnd = holeStart < 0 ? -1 : template.IndexOf('}', holeStart + 1);
            if (holeEnd < 0)
            {
                break;
            }

            message.Append(template, position, holeStart - position);
            string holeName = template.Substring(holeStart + 1, holeEnd - holeStart - 1);
            if (state.TryGetValue(holeName, out object? value))
            {
                message.Append(FormatTemplateValue(value));
            }
            else
            {
                message.Append(template, holeStart, holeEnd - holeStart + 1);
            }

            position = holeEnd + 1;
        }

        message.Append(template, position, template.Length - position);
        return message.ToString();
    }

    /// <summary>
    /// Formats a value substituted into a rendered message, independently of the current culture.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted value.</returns>
    private static string? FormatTemplateValue(object? value)
    {
        return value switch
        {
            null => "(null)",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    /// <summary>
    /// Subscribes to the WebDriverBiDi event source at the configured minimum level.
    /// </summary>
    /// <remarks>
    /// Called from each constructor, after the minimum level is assigned, to correct the subscription for a
    /// source that already existed when this listener was created: the base constructor enabled it through
    /// <see cref="OnEventSourceCreated"/> while the level was still the default of
    /// <see cref="EventLevel.LogAlways"/>, which leaves the source reporting itself enabled for every event.
    /// Each such event is then formatted and dispatched only for the listener to discard it. Re-subscribing
    /// lowers the source's own level so that work is never done. Enabling an already-enabled source updates
    /// its level rather than adding a second subscription, so this is safe on either ordering.
    /// </remarks>
    private void EnableConfiguredEvents()
    {
        this.EnableEvents(WebDriverBiDiEventSource.RaiseEvent, this.minimumLevel);
    }

    /// <summary>
    /// Gets the named form of an event's message template, rewriting it on the first event with that ID.
    /// </summary>
    /// <param name="eventData">The event whose message template to get.</param>
    /// <param name="payloadNames">The names of the event's payload properties, in payload order.</param>
    /// <param name="payloadCount">The number of payload properties.</param>
    /// <returns>The named message template, or <see langword="null"/> when the event declares no template.</returns>
    private string? GetNamedMessageTemplate(EventWrittenEventArgs eventData, ReadOnlyCollection<string>? payloadNames, int payloadCount)
    {
        string? template = eventData.Message;
        if (string.IsNullOrEmpty(template))
        {
            return null;
        }

        if (this.namedMessageTemplates.TryGetValue(eventData.EventId, out string? namedTemplate))
        {
            return namedTemplate;
        }

        namedTemplate = ConvertToNamedTemplate(template!, payloadNames, payloadCount);
        this.namedMessageTemplates.TryAdd(eventData.EventId, namedTemplate);
        return namedTemplate;
    }
}
