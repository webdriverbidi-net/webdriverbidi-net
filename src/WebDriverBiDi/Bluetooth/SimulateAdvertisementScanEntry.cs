// <copyright file="SimulateAdvertisementScanEntry.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides information about a record retrieved when scanning for Bluetooth devices.
/// </summary>
public class SimulateAdvertisementScanEntry
{
    private string deviceAddress;
    private double receivedSignalStrengthIndicator;
    private ScanRecord scanRecord;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulateAdvertisementScanEntry"/> class.
    /// </summary>
    /// <param name="deviceAddress">The physical address of the simulated Bluetooth peripheral.</param>
    /// <param name="receivedSignalStrengthIndicator">The simulated strength of signal, as expressed in dBm.</param>
    /// <param name="scanRecord">The <see cref="ScanRecord"/> representing the data for this scan for Bluetooth devices.</param>
    public SimulateAdvertisementScanEntry(string deviceAddress, double receivedSignalStrengthIndicator, ScanRecord scanRecord)
    {
        this.deviceAddress = deviceAddress;
        this.receivedSignalStrengthIndicator = receivedSignalStrengthIndicator;
        this.scanRecord = scanRecord;
    }

    /// <summary>
    /// Gets or sets the physical address of the simulated Bluetooth device, or a prefix of it.
    /// </summary>
    [JsonPropertyName("address")]
    public string DeviceAddress { get => this.deviceAddress; set => this.deviceAddress = value; }

    /// <summary>
    /// Gets or sets the list of Service UUIDs that this scan record says the Bluetooth device's GATT server supports.
    /// </summary>
    [JsonPropertyName("rssi")]
    public double Rssi { get => this.receivedSignalStrengthIndicator; set => this.receivedSignalStrengthIndicator = value; }

    /// <summary>
    /// Gets or sets the local name of the Bluetooth device, or a prefix of it.
    /// </summary>
    [JsonPropertyName("scanRecord")]
    public ScanRecord ScanRecord { get => this.scanRecord; set => this.scanRecord = value; }
}
