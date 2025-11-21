// <copyright file="SimulateAdvertisementCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.simulateAdvertisement command.
/// </summary>
public class SimulateAdvertisementCommandParameters : CommandParameters<SimulateAdvertisementCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulateAdvertisementCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to simulate the advertisement of Bluetooth peripheral.</param>
    /// <param name="scanEntry">The <see cref="SimulateAdvertisementScanEntry"/> object representing data about this entry scanning for peripherals.</param>
    public SimulateAdvertisementCommandParameters(string browsingContextId, SimulateAdvertisementScanEntry scanEntry)
    {
        this.BrowsingContextId = browsingContextId;
        this.ScanEntry = scanEntry;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "bluetooth.simulateAdvertisement";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to simulate the already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the address of the simulated already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("scanEntry")]
    public SimulateAdvertisementScanEntry ScanEntry { get; set; }
}
