// <copyright file="CharacteristicProperties.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides information about a Bluetooth device characteristic.
/// </summary>
public class CharacteristicProperties
{
    private bool? broadcast;
    private bool? read;
    private bool? writeWithoutResponse;
    private bool? write;
    private bool? notify;
    private bool? indicate;
    private bool? authenticatedSignedWrites;
    private bool? extendedProperties;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacteristicProperties"/> class.
    /// </summary>
    public CharacteristicProperties()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether the characteristic is to broadcast.
    /// </summary>
    [JsonPropertyName("broadcast")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsBroadcast { get => this.broadcast; set => this.broadcast = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the characteristic is to read.
    /// </summary>
    [JsonPropertyName("read")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsRead { get => this.read; set => this.read = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the characteristic is to write without a response.
    /// </summary>
    [JsonPropertyName("writeWithoutResponse")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsWriteWithoutResponse { get => this.writeWithoutResponse; set => this.writeWithoutResponse = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the characteristic is to write.
    /// </summary>
    [JsonPropertyName("write")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsWrite { get => this.write; set => this.write = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the characteristic is to write.
    /// </summary>
    [JsonPropertyName("notify")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsNotify { get => this.notify; set => this.notify = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the characteristic is to indicate.
    /// </summary>
    [JsonPropertyName("indicate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsIndicate { get => this.indicate; set => this.indicate = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the characteristic is to use authenticated signed writes.
    /// </summary>
    [JsonPropertyName("authenticatedSignedWrites")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsAuthenticatedSignedWrites { get => this.authenticatedSignedWrites; set => this.authenticatedSignedWrites = value; }

    /// <summary>
    /// Gets or sets a value indicating whether the characteristic is extended properties.
    /// </summary>
    [JsonPropertyName("extendedProperties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsExtendedProperties { get => this.extendedProperties; set => this.extendedProperties = value; }
}
