// <copyright file="EventMessage.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Text.Json.Serialization;

/// <summary>
/// Deserializes a message that represents an event as defined by the WebDriver Bidi protocol.
/// </summary>
public abstract class EventMessage : Message
{
    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    [JsonPropertyName("method")]
    [JsonInclude]
    public string EventName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the data for the event.
    /// </summary>
    [JsonIgnore]
    public abstract object EventData { get; }
}
