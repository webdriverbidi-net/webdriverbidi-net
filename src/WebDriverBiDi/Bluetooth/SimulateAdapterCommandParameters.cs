// <copyright file="SimulateAdapterCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.simulateAdapter command.
/// </summary>
public class SimulateAdapterCommandParameters : CommandParameters<EmptyResult>
{
    private string browsingContextId;
    private AdapterState state;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulateAdapterCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to simulate the Bluetooth adapter.</param>
    /// <param name="state">The simulated state of the Bluetooth adapter.</param>
    public SimulateAdapterCommandParameters(string browsingContextId, AdapterState state)
    {
        this.browsingContextId = browsingContextId;
        this.state = state;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "bluetooth.simulateAdapter";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to simulate the Bluetooth adapter.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get => this.browsingContextId;  set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the simulated state the Bluetooth adapter.
    /// </summary>
    [JsonPropertyName("state")]
    public AdapterState State { get => this.state;  set => this.state = value; }
}
