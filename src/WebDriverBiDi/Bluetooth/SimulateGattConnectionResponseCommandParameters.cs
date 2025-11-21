// <copyright file="SimulateGattConnectionResponseCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.simulateGattConnectionResponse command.
/// </summary>
public class SimulateGattConnectionResponseCommandParameters : CommandParameters<SimulateGattConnectionResponseCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulateGattConnectionResponseCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to simulate the GATT connection response of the Bluetooth adapter.</param>
    /// <param name="address">The address of the simulated device.</param>
    /// <param name="code">The code of the simulated response.</param>
    public SimulateGattConnectionResponseCommandParameters(string browsingContextId, string address, uint code)
    {
        this.BrowsingContextId = browsingContextId;
        this.Address = address;
        this.Code = code;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "bluetooth.simulateGattConnectionResponse";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to simulate the GATT response for the Bluetooth adapter.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the address of the device for which to simulate the GATT response.
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; }

    /// <summary>
    /// Gets or sets the code of the simulated GATT response.
    /// </summary>
    [JsonPropertyName("code")]
    public uint Code { get; set; }
}
