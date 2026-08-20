// <copyright file="PayloadlessEventSource.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Logging.TestUtilities;

using System.Diagnostics.Tracing;

/// <summary>
/// A self-describing EventSource that deliberately shares the "WebDriverBiDi" name so that
/// <see cref="WebDriverBiDiEventSourceLogger"/> subscribes to it, and that raises an event
/// carrying no payload at all.
/// </summary>
/// <remarks>
/// <para>
/// The production listener guards its payload copy with
/// <c>eventData.PayloadNames != null &amp;&amp; eventData.Payload != null</c>. Manifest-based
/// events — every event on the real <c>WebDriverBiDiEventSource</c> — can never satisfy that
/// guard's null arms: the runtime hands the listener empty collections rather than nulls, even
/// for an event declared with no parameters. Only the self-describing (TraceLogging) write path
/// leaves both properties null, and only when the event carries no payload, which is what
/// <see cref="RaisePayloadlessEvent"/> produces.
/// </para>
/// <para>
/// Duplicate EventSource names are permitted in a single process; both instances resolve to the
/// same name-derived GUID and are dispatched to every listener that enabled the name.
/// </para>
/// </remarks>
public sealed class PayloadlessEventSource : EventSource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PayloadlessEventSource"/> class.
    /// </summary>
    public PayloadlessEventSource()
        : base("WebDriverBiDi", EventSourceSettings.EtwSelfDescribingEventFormat)
    {
    }

    /// <summary>
    /// Raises an event whose <c>PayloadNames</c> and <c>Payload</c> are both null.
    /// </summary>
    /// <param name="eventName">The name to give the raised event.</param>
    public void RaisePayloadlessEvent(string eventName)
    {
        this.Write(eventName);
    }
}
