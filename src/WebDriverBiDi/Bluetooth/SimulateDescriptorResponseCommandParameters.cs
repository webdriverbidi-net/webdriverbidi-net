// <copyright file="SimulateDescriptorResponseCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.simulateDescriptorResponse command.
/// </summary>
public class SimulateDescriptorResponseCommandParameters : CommandParameters<EmptyResult>
{
    private string browsingContextId;
    private string address;
    private string serviceUuid;
    private string characteristicUuid;
    private string descriptorUuid;
    private SimulateDescriptorResponseType type;
    private uint code;
    private List<uint>? data;

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
        this.browsingContextId = browsingContextId;
        this.address = address;
        this.serviceUuid = serviceUuid;
        this.characteristicUuid = characteristicUuid;
        this.descriptorUuid = descriptorUuid;
        this.type = type;
        this.code = code;
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
    public string BrowsingContextId { get => this.browsingContextId;  set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the address of the device for which to simulate the descriptor response.
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
    /// Gets or sets the UUID of the descriptor to simulate.
    /// </summary>
    [JsonPropertyName("descriptorUuid")]
    public string DescriptorUuid { get => this.descriptorUuid; set => this.descriptorUuid = value; }

    /// <summary>
    /// Gets or sets the type of simulation of the descriptor response.
    /// </summary>
    [JsonPropertyName("type")]
    public SimulateDescriptorResponseType Type { get => this.type; set => this.type = value; }

    /// <summary>
    /// Gets or sets the code of the simulated descriptor response.
    /// </summary>
    [JsonPropertyName("code")]
    public uint Code { get => this.code; set => this.code = value; }

    /// <summary>
    /// Gets or sets the data for the simulated descriptor response.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<uint>? Data { get => this.data; set => this.data = value; }
}
