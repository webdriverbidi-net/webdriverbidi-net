// <copyright file="ScanRecord.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides information about a record retrieved when scanning for Bluetooth devices.
/// </summary>
public class ScanRecord
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScanRecord"/> class.
    /// </summary>
    public ScanRecord()
    {
    }

    /// <summary>
    /// Gets or sets the local name of the Bluetooth device, or a prefix of it.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the list of Service UUIDs that this scan record says the Bluetooth device's GATT server supports.
    /// </summary>
    [JsonPropertyName("uuids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UUIDs { get; set; }

    /// <summary>
    /// Gets or sets the local name of the Bluetooth device, or a prefix of it.
    /// </summary>
    [JsonPropertyName("appearance")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? Appearance { get; set; }

    /// <summary>
    /// Gets or sets the list of Service UUIDs that this scan record says the Bluetooth device's GATT server supports.
    /// </summary>
    [JsonPropertyName("manufacturerData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<BluetoothManufacturerData>? ManufacturerData { get; set; }
}
