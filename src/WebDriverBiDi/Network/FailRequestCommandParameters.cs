// <copyright file="FailRequestCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.failRequest command.
/// </summary>
public class FailRequestCommandParameters : CommandParameters<FailRequestCommandResult>
{
    private string requestId;

    /// <summary>
    /// Initializes a new instance of the <see cref="FailRequestCommandParameters" /> class.
    /// </summary>
    /// <param name="requestId">The ID of the request to fail.</param>
    public FailRequestCommandParameters(string requestId)
    {
        this.requestId = requestId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.failRequest";

    /// <summary>
    /// Gets or sets the ID of the request to fail..
    /// </summary>
    [JsonPropertyName("request")]
    public string RequestId { get => this.requestId; set => this.requestId = value; }
}
