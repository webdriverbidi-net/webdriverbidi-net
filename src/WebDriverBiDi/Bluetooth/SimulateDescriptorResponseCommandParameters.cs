// <copyright file="SimulateDescriptorResponseCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.simulateDescriptorResponse command.
/// </summary>
public class SimulateDescriptorResponseCommandParameters : CommandParameters<SimulateDescriptorResponseCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulateDescriptorResponseCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to simulation of the Bluetooth adapter.</param>
    /// <param name="address">The address of the simulated device.</param>
    /// <param name="serviceUuid">The UUID of the service to simulate.</param>
    /// <param name="characteristicUuid">The UUID of the characteristic to simulate.</param>
    /// <param name="descriptorUuid">The UUID of the descriptor to simulate.</param>
    /// <param name="type">The <see cref="SimulateDescriptorResponseType"/> value for the type of simulation for the characteristic response.</param>
    /// <param name="code">The code of the simulated characteristic response.</param>
    public SimulateDescriptorResponseCommandParameters(string browsingContextId, string address, string serviceUuid, string characteristicUuid, string descriptorUuid, SimulateDescriptorResponseType type, uint code)
    {
        this.BrowsingContextId = browsingContextId;
        this.Address = address;
        this.ServiceUuid = serviceUuid;
        this.CharacteristicUuid = characteristicUuid;
        this.DescriptorUuid = descriptorUuid;
        this.Type = type;
        this.Code = code;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "bluetooth.simulateDescriptorResponse";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to simulate a descriptor response on the Bluetooth adapter.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the address of the device for which to simulate the descriptor response.
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
    /// Gets or sets the UUID of the descriptor to simulate.
    /// </summary>
    [JsonPropertyName("descriptorUuid")]
    public string DescriptorUuid { get; set; }

    /// <summary>
    /// Gets or sets the type of simulation of the descriptor response.
    /// </summary>
    [JsonPropertyName("type")]
    public SimulateDescriptorResponseType Type { get; set; }

    /// <summary>
    /// Gets or sets the code of the simulated descriptor response.
    /// </summary>
    [JsonPropertyName("code")]
    public uint Code { get; set; }

    /// <summary>
    /// Gets or sets the data for the simulated descriptor response.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<uint>? Data { get; set; }
}
