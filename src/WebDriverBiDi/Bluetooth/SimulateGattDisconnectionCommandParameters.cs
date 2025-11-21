// <copyright file="SimulateGattDisconnectionCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.simulateGattDisconnection command.
/// </summary>
public class SimulateGattDisconnectionCommandParameters : CommandParameters<SimulateGattDisconnectionCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulateGattDisconnectionCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to simulate the GATT connection response of the Bluetooth adapter.</param>
    /// <param name="address">The address of the simulated device.</param>
    public SimulateGattDisconnectionCommandParameters(string browsingContextId, string address)
    {
        this.BrowsingContextId = browsingContextId;
        this.Address = address;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "bluetooth.simulateGattDisconnection";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to simulate the GATT disconnection for the Bluetooth adapter.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the address of the device for which to simulate the GATT disconnection.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; }
}
