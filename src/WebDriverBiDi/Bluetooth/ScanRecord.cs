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
    /// Gets the list of Service UUIDs that this scan record says the Bluetooth device's GATT server supports.
    /// </summary>
    /// <remarks>
    /// This property is optional in the protocol, and omitting it has the same meaning as sending an
    /// empty array: the advertising event the remote end fires starts from an empty list of UUIDs and
    /// is populated only from the entries this record carries. An empty list therefore means "not
    /// specified": the property is omitted from the JSON payload entirely. Add entries to the list to
    /// populate it.
    /// </remarks>
    [JsonIgnore]
    public List<string> UUIDs { get; } = [];

    /// <summary>
    /// Gets or sets the appearance value of the Bluetooth device.
    /// </summary>
    [JsonPropertyName("appearance")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? Appearance { get; set; }

    /// <summary>
    /// Gets the list of manufacturer data for this scan record.
    /// </summary>
    /// <remarks>
    /// This property is optional in the protocol, and omitting it has the same meaning as sending an
    /// empty array: the advertising event the remote end fires starts from an empty manufacturer data
    /// map and is populated only from the entries this record carries. An empty list therefore means
    /// "not specified": the property is omitted from the JSON payload entirely. Add entries to the
    /// list to populate it.
    /// </remarks>
    [JsonIgnore]
    public List<BluetoothManufacturerData> ManufacturerData { get; } = [];

    /// <summary>
    /// Gets the list of Service UUIDs for this scan record, for serialization purposes.
    /// </summary>
    [JsonPropertyName("uuids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableUUIDs
    {
        get
        {
            if (this.UUIDs.Count == 0)
            {
                return null;
            }

            return this.UUIDs;
        }
    }

    /// <summary>
    /// Gets the list of manufacturer data for this scan record, for serialization purposes.
    /// </summary>
    [JsonPropertyName("manufacturerData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<BluetoothManufacturerData>? SerializableManufacturerData
    {
        get
        {
            if (this.ManufacturerData.Count == 0)
            {
                return null;
            }

            return this.ManufacturerData;
        }
    }
}
