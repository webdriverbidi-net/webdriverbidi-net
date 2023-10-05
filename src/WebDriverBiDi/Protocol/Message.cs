// <copyright file="Message.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Object containing data about a WebDriver Bidi message.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Message
{
    private string type = string.Empty;
    private Dictionary<string, object?> writableAdditionalData = new();
    private ReceivedDataDictionary additionalData = ReceivedDataDictionary.EmptyDictionary;

    /// <summary>
    /// Gets the type of message.
    /// </summary>
    // TODO: Uncomment this attribute when the browser stable channels
    // have the message type property implemented.
    // [JsonRequired]
    [JsonProperty("type")]
    public string Type { get => this.type; internal set => this.type = value; }

    /// <summary>
    /// Gets read-only dictionary of additional properties deserialized with this message.
    /// </summary>
    public ReceivedDataDictionary AdditionalData
    {
        get
        {
            if (this.writableAdditionalData.Count > 0 && this.additionalData.Count == 0)
            {
                this.additionalData = new ReceivedDataDictionary(this.writableAdditionalData);
            }

            return this.additionalData;
        }
    }

    /// <summary>
    /// Gets additional properties deserialized with this message.
    /// </summary>
    [JsonExtensionData]
    [JsonConverter(typeof(ReceivedDataJsonConverter))]
    internal Dictionary<string, object?> SerializableAdditionalData { get => this.writableAdditionalData; private set => this.writableAdditionalData = value; }
}
