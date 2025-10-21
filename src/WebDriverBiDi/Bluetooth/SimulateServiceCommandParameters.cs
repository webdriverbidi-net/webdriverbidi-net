// <copyright file="SimulateServiceCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.simulateService command.
/// </summary>
public class SimulateServiceCommandParameters : CommandParameters<SimulateServiceCommandResult>
{
    private string browsingContextId;
    private string address;
    private string serviceUuid;
    private SimulateServiceType type;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulateServiceCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to simulation of the Bluetooth adapter.</param>
    /// <param name="address">The address of the simulated device.</param>
    /// <param name="serviceUuid">The UUID of the service to simulate.</param>
    /// <param name="type">The <see cref="SimulateServiceType"/> value for the type of simulation for the service.</param>
    public SimulateServiceCommandParameters(string browsingContextId, string address, string serviceUuid, SimulateServiceType type)
    {
        this.browsingContextId = browsingContextId;
        this.address = address;
        this.serviceUuid = serviceUuid;
        this.type = type;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "bluetooth.simulateService";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to simulate a service on the Bluetooth adapter.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get => this.browsingContextId;  set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the address of the device for which to simulate the service.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get => this.address; set => this.address = value; }

    /// <summary>
    /// Gets or sets the UUID of the service to simulate.
    /// </summary>
    [JsonPropertyName("uuid")]
    public string ServiceUuid { get => this.serviceUuid; set => this.serviceUuid = value; }

    /// <summary>
    /// Gets or sets the type of simulation of the service.
    /// </summary>
    [JsonPropertyName("type")]
    public SimulateServiceType Type { get => this.type; set => this.type = value; }
}
