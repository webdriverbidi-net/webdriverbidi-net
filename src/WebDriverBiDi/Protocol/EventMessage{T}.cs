// <copyright file="EventMessage{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Text.Json.Serialization;

/// <summary>
/// Deserializes a message that represents an event as defined by the WebDriver Bidi protocol where the event data type is known.
/// </summary>
/// <typeparam name="T">The type of data contained in the event.</typeparam>
public class EventMessage<T> : EventMessage
{
    /// <summary>
    /// Gets the data associated with the event.
    /// </summary>
    [JsonIgnore]
    public override object EventData => this.SerializableData!;

    /// <summary>
    /// Gets the data of the event for serialization purposes.
    /// </summary>
    [JsonPropertyName("params")]
    [JsonRequired]
    [JsonInclude]
    internal T? SerializableData { get; set; }
}
