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
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulateAdvertisementScanEntry"/> class.
    /// </summary>
    /// <param name="deviceAddress">The physical address of the simulated Bluetooth peripheral.</param>
    /// <param name="receivedSignalStrengthIndicator">The simulated strength of signal, as expressed in dBm.</param>
    /// <param name="scanRecord">The <see cref="ScanRecord"/> representing the data for this scan for Bluetooth devices.</param>
    public SimulateAdvertisementScanEntry(string deviceAddress, double receivedSignalStrengthIndicator, ScanRecord scanRecord)
    {
        this.DeviceAddress = deviceAddress;
        this.Rssi = receivedSignalStrengthIndicator;
        this.ScanRecord = scanRecord;
    }

    /// <summary>
    /// Gets or sets the physical address of the simulated Bluetooth device, or a prefix of it.
    /// </summary>
    [JsonPropertyName("address")]
    public string DeviceAddress { get; set; }

    /// <summary>
    /// Gets or sets the received signal strength indicator (RSSI) of the simulated Bluetooth device, in dBm.
    /// </summary>
    [JsonPropertyName("rssi")]
    public double Rssi { get; set; }

    /// <summary>
    /// Gets or sets the scan record data for this advertisement scan entry.
    /// </summary>
    [JsonPropertyName("scanRecord")]
    public ScanRecord ScanRecord { get; set; }
}
