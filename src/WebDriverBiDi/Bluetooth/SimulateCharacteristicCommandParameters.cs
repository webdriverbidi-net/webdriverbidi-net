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
    private string browsingContextId;
    private string address;
    private string serviceUuid;
    private string characteristicUuid;
    private CharacteristicProperties? characteristicProperties;
    private SimulateCharacteristicType type;

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
        this.browsingContextId = browsingContextId;
        this.address = address;
        this.serviceUuid = serviceUuid;
        this.characteristicUuid = characteristicUuid;
        this.type = type;
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
    public string BrowsingContextId { get => this.browsingContextId;  set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the address of the device for which to simulate the characteristic.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get => this.address; set => this.address = value; }

    /// <summary>
    /// Gets or sets the UUID of the service to simulate.
    /// </summary>
    [JsonPropertyName("serviceUuid")]
    public string ServiceUuid { get => this.serviceUuid; set => this.serviceUuid = value; }

    /// <summary>
    /// Gets or sets the UUID of the characteristic to simulate.
    /// </summary>
    [JsonPropertyName("characteristicUuid")]
    public string CharacteristicUuid { get => this.characteristicUuid; set => this.characteristicUuid = value; }

    /// <summary>
    /// Gets or sets the properties of the characteristic to simulate.
    /// </summary>
    [JsonPropertyName("characteristicProperties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CharacteristicProperties? CharacteristicProperties { get => this.characteristicProperties; set => this.characteristicProperties = value; }

    /// <summary>
    /// Gets or sets the type of simulation of the characteristic.
    /// </summary>
    [JsonPropertyName("type")]
    public SimulateCharacteristicType Type { get => this.type; set => this.type = value; }
}
