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
    private string browsingContextId;
    private string address;
    private string name;
    private List<BluetoothManufacturerData> manufacturerData = new();
    private List<string> knownServiceUUIDs = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatePreconnectedPeripheralCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to simulate the already-connected Bluetooth peripheral.</param>
    /// <param name="address">The address of the Bluetooth peripheral.</param>
    /// <param name="name">The name of the Bluetooth peripheral.</param>
    public SimulatePreconnectedPeripheralCommandParameters(string browsingContextId, string address, string name)
    {
        this.browsingContextId = browsingContextId;
        this.address = address;
        this.name = name;
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
    public string BrowsingContextId { get => this.browsingContextId;  set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the address of the simulated already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get => this.address;  set => this.address = value; }

    /// <summary>
    /// Gets or sets the display name of the simulated already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get => this.name;  set => this.name = value; }

    /// <summary>
    /// Gets the list of all of the manufacturer data for the simulated already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("manufacturerData")]
    public List<BluetoothManufacturerData> ManufacturerData => this.manufacturerData;

    /// <summary>
    /// Gets the list of all of the known service UUIDs for the simulated already-connected Bluetooth peripheral.
    /// </summary>
    [JsonPropertyName("knownServiceUuids")]
    public List<string> KnownServiceUUIDs => this.knownServiceUUIDs;
}
