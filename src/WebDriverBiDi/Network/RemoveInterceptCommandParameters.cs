// <copyright file="RemoveInterceptCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.removeIntercept command.
/// </summary>
public class RemoveInterceptCommandParameters : CommandParameters<RemoveInterceptCommandResult>
{
    private string interceptId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveInterceptCommandParameters" /> class.
    /// </summary>
    /// <param name="interceptId">The ID of the intercept to remove.</param>
    public RemoveInterceptCommandParameters(string interceptId)
    {
        this.interceptId = interceptId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.removeIntercept";

    /// <summary>
    /// Gets or sets the ID of the intercept to remove.
    /// </summary>
    [JsonPropertyName("intercept")]
    public string InterceptId { get => this.interceptId; set => this.interceptId = value; }
}
