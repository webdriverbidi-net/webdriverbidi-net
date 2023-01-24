// <copyright file="WebDriverBidiMessage.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Object containing data about a WebDriver Bidi message.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class WebDriverBidiMessage
{
    private Dictionary<string, object?> additionalData = new();

    /// <summary>
    /// Gets a value indicating whether the message received is an error.
    /// </summary>
    public virtual bool IsError => false;

    /// <summary>
    /// Gets a value indicating whether the message received is an event.
    /// </summary>
    public virtual bool IsEvent => false;

    /// <summary>
    /// Gets additional properties to be serialized with this command.
    /// </summary>
    [JsonExtensionData]
    [JsonConverter(typeof(ResponseValueJsonConverter))]
    public Dictionary<string, object?> AdditionalData { get => this.additionalData; private set => this.additionalData = value; }
}