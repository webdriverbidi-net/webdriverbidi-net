// <copyright file="SimulatePreconnectedPeripheralCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.simulatePreconnectedPeripheral command.
/// </summary>
public class SimulatePreconnectedPeripheralCommandParameters : CommandParameters<SimulatePreconnectedPeripheralCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatePreconnectedPeripheralCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to simulate the already-connected Bluetooth peripheral.</param>
    /// <param name="address">The address of the Bluetooth peripheral.</param>
    /// <param name="name">The name of the Bluetooth peripheral.</param>
    public SimulatePreconnectedPeripheralCommandParameters(string browsingContextId, string address, string name)
    {
        this.BrowsingContextId = browsingContextId;
        this.Address = address;
        this.Name = name;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "bluetooth.simulatePreconnectedPeripheral";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to simulate the already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the address of the simulated already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the display name of the simulated already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets the list of all of the manufacturer data for the simulated already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("manufacturerData")]
    public List<BluetoothManufacturerData> ManufacturerData { get; } = [];

    /// <summary>
    /// Gets the list of all of the known service UUIDs for the simulated already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("knownServiceUuids")]
    public List<string> KnownServiceUUIDs { get; } = [];
}
