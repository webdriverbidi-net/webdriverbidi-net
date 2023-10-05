// <copyright file="EventMessage.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using Newtonsoft.Json;

/// <summary>
/// Deserializes a message that represents an event as defined by the WebDriver Bidi protocol.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public abstract class EventMessage : Message
{
    private string eventName = string.Empty;

    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    [JsonProperty("method")]
    public string EventName { get => this.eventName; private set => this.eventName = value; }

    /// <summary>
    /// Gets the data for the event.
    /// </summary>
    public abstract object EventData { get; }
}
