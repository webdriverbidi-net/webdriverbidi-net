// <copyright file="SimulateCharacteristicCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.simulateCharacteristic command.
/// </summary>
public class SimulateCharacteristicCommandParameters : CommandParameters<SimulateCharacteristicCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulateCharacteristicCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to simulation of the Bluetooth adapter.</param>
    /// <param name="address">The address of the simulated device.</param>
    /// <param name="serviceUuid">The UUID of the service to simulate.</param>
    /// <param name="characteristicUuid">The UUID of the characteristic to simulate.</param>
    /// <param name="type">The <see cref="SimulateCharacteristicType"/> value for the type of simulation for the characteristic.</param>
    public SimulateCharacteristicCommandParameters(string browsingContextId, string address, string serviceUuid, string characteristicUuid, SimulateCharacteristicType type)
    {
        this.BrowsingContextId = browsingContextId;
        this.Address = address;
        this.ServiceUuid = serviceUuid;
        this.CharacteristicUuid = characteristicUuid;
        this.Type = type;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "bluetooth.simulateCharacteristic";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to simulate a characteristic on the Bluetooth adapter.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the address of the device for which to simulate the characteristic.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the UUID of the service to simulate.
    /// </summary>
    [JsonPropertyName("serviceUuid")]
    public string ServiceUuid { get; set; }

    /// <summary>
    /// Gets or sets the UUID of the characteristic to simulate.
    /// </summary>
    [JsonPropertyName("characteristicUuid")]
    public string CharacteristicUuid { get; set; }

    /// <summary>
    /// Gets or sets the properties of the characteristic to simulate.
    /// </summary>
    [JsonPropertyName("characteristicProperties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CharacteristicProperties? CharacteristicProperties { get; set; }

    /// <summary>
    /// Gets or sets the type of simulation of the characteristic.
    /// </summary>
    [JsonPropertyName("type")]
    public SimulateCharacteristicType Type { get; set; }
}
