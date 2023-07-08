// <copyright file="ContinueWithAuthCommandParameters.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the network.continueResponse command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ContinueWithAuthCommandParameters : CommandParameters<EmptyResult>
{
    private string requestId;
    private ContinueWithAuthActionType action = ContinueWithAuthActionType.Default;
    private AuthCredentials credentials = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContinueWithAuthCommandParameters" /> class.
    /// </summary>
    /// <param name="requestId">The ID of the request to continue.</param>
    public ContinueWithAuthCommandParameters(string requestId)
    {
        this.requestId = requestId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "network.continueWithAuth";

    /// <summary>
    /// Gets or sets the ID of the request to continue.
    /// </summary>
    [JsonProperty("request")]
    public string RequestId { get => this.requestId; set => this.requestId = value; }

    /// <summary>
    /// Gets or sets the action to use with continuing this request.
    /// </summary>
    [JsonProperty("action")]
    public ContinueWithAuthActionType Action { get => this.action; set => this.action = value; }

    /// <summary>
    /// Gets or sets the credentials to be used when continuing this request.
    /// Credentials are only sent when the action is set to <see cref="ContinueWithAuthActionType.ProvideCredentials" />.
    /// </summary>
    public AuthCredentials Credentials { get => this.credentials; set => this.credentials = value; }

    /// <summary>
    /// Gets the credentials to be used for continuing the request for authorization purposes.
    /// Credentials are only sent when the action is set to <see cref="ContinueWithAuthActionType.ProvideCredentials" />.
    /// </summary>
    [JsonProperty("credentials", NullValueHandling = NullValueHandling.Ignore)]
    internal AuthCredentials? SerializableCredentials => this.action == ContinueWithAuthActionType.ProvideCredentials ? this.credentials : null;
}