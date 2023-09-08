// <copyright file="FailRequestCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the network.failRequest command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class FailRequestCommandParameters : CommandParameters<EmptyResult>
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
    public override string MethodName => "network.failRequest";

    /// <summary>
    /// Gets or sets the ID of the request to fail..
    /// </summary>
    [JsonProperty("request")]
    public string RequestId { get => this.requestId; set => this.requestId = value; }
}