// <copyright file="DisableSimulationCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the bluetooth.disableSimulation command.
/// </summary>
public class DisableSimulationCommandParameters : CommandParameters<DisableSimulationCommandResult>
{
    private string browsingContextId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisableSimulationCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to disable simulation of the Bluetooth adapter.</param>
    public DisableSimulationCommandParameters(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "bluetooth.disableSimulation";

    /// <summary>
    /// Gets or sets the ID of the browsing context for which to simulate the Bluetooth adapter.
    /// </summary>
    [JsonPropertyName("context")]
    public string BrowsingContextId { get => this.browsingContextId;  set => this.browsingContextId = value; }
}
