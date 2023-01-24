// <copyright file="EventMessage{T}.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using Newtonsoft.Json;

/// <summary>
/// Deserializes a message that represents an event as defined by the WebDriver Bidi protocol where the event data type is known.
/// </summary>
/// <typeparam name="T">The type of data contained in the event.</typeparam>
[JsonObject(MemberSerialization.OptIn)]
public class EventMessage<T> : EventMessage
{
    private T? data;

    /// <summary>
    /// Gets the data associated with the event.
    /// </summary>
    public override object EventData => this.data!;

    /// <summary>
    /// Gets the data of the event for serialization purpopses.
    /// </summary>
    [JsonProperty("params")]
    [JsonRequired]
    internal T? SerializableData { get => this.data; private set => this.data = value; }
}