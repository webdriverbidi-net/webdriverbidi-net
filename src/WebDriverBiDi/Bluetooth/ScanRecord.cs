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
    private string? name;
    private List<string>? uuids;
    private uint? appearance;
    private List<BluetoothManufacturerData>? manufacturerData;

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
    public string? Name { get => this.name; set => this.name = value; }

    /// <summary>
    /// Gets or sets the list of Service UUIDs that this scan record says the Bluetooth device's GATT server supports.
    /// </summary>
    [JsonPropertyName("uuids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UUIDs { get => this.uuids; set => this.uuids = value; }

    /// <summary>
    /// Gets or sets the local name of the Bluetooth device, or a prefix of it.
    /// </summary>
    [JsonPropertyName("appearance")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? Appearance { get => this.appearance; set => this.appearance = value; }

    /// <summary>
    /// Gets or sets the list of Service UUIDs that this scan record says the Bluetooth device's GATT server supports.
    /// </summary>
    [JsonPropertyName("manufacturerData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<BluetoothManufacturerData>? ManufacturerData { get => this.manufacturerData; set => this.manufacturerData = value; }
}
