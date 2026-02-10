// <copyright file="CharacteristicEventGeneratedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for events raised when a Bluetooth GATT connection is attempted.
/// </summary>
public record CharacteristicEventGeneratedEventArgs : WebDriverBiDiEventArgs
{
    [JsonConstructor]
    private CharacteristicEventGeneratedEventArgs()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context generating the characteristic event.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string BrowsingContextId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the address of the device generating the characteristic event..
    /// </summary>
    [JsonPropertyName("address")]
    [JsonRequired]
    [JsonInclude]
    public string Address { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the UUID of the service generating the characteristic event.
    /// </summary>
    [JsonPropertyName("serviceUuid")]
    [JsonRequired]
    [JsonInclude]
    public string ServiceUuid { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the UUID of the characteristic generating the characteristic event.
    /// </summary>
    [JsonPropertyName("characteristicUuid")]
    [JsonRequired]
    [JsonInclude]
    public string CharacteristicUuid { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the type of the characteristic event.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonRequired]
    [JsonInclude]
    public CharacteristicEventGeneratedType Type { get; internal set; } = CharacteristicEventGeneratedType.Read;

    /// <summary>
    /// Gets the read-only data for the event, if any.
    /// </summary>
    [JsonIgnore]
    public IList<uint>? Data => this.SerializableData?.AsReadOnly();

    /// <summary>
    /// Gets or sets the read-only data for the event.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonInclude]
    internal List<uint>? SerializableData { get; set; }
}
