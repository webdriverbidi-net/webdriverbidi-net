// <copyright file="ContinueWithAuthCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.continueResponse command.
/// </summary>
public class ContinueWithAuthCommandParameters : CommandParameters<ContinueWithAuthCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContinueWithAuthCommandParameters" /> class.
    /// </summary>
    /// <param name="requestId">The ID of the request to continue.</param>
    public ContinueWithAuthCommandParameters(string requestId)
    {
        this.RequestId = requestId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.continueWithAuth";

    /// <summary>
    /// Gets or sets the ID of the request to continue.
    /// </summary>
    [JsonPropertyName("request")]
    public string RequestId { get; set; }

    /// <summary>
    /// Gets or sets the action to use with continuing this request.
    /// </summary>
    [JsonPropertyName("action")]
    public ContinueWithAuthActionType Action { get; set; } = ContinueWithAuthActionType.Default;

    /// <summary>
    /// Gets or sets the credentials to be used when continuing this request.
    /// Credentials are only sent when the action is set to <see cref="ContinueWithAuthActionType.ProvideCredentials" />.
    /// </summary>
    [JsonIgnore]
    public AuthCredentials Credentials { get; set; } = new();

    /// <summary>
    /// Gets the credentials to be used for continuing the request for authorization purposes.
    /// Credentials are only sent when the action is set to <see cref="ContinueWithAuthActionType.ProvideCredentials" />.
    /// </summary>
    [JsonPropertyName("credentials")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal AuthCredentials? SerializableCredentials => this.Action == ContinueWithAuthActionType.ProvideCredentials ? this.Credentials : null;
}
