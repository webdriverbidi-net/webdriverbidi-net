// <copyright file="ContinueResponseCommandParameters.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the network.continueResponse command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ContinueResponseCommandParameters : CommandParameters<EmptyResult>
{
    private string requestId;
    private AuthCredentials? credentials;
    private List<Header>? headers;
    private List<SetCookieHeader>? cookieHeaders;
    private string? reasonPhrase;
    private ulong? statusCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContinueResponseCommandParameters" /> class.
    /// </summary>
    /// <param name="requestId">The ID of the request to continue.</param>
    public ContinueResponseCommandParameters(string requestId)
    {
        this.requestId = requestId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "network.continueResponse";

    /// <summary>
    /// Gets or sets the ID of the request to continue.
    /// </summary>
    [JsonProperty("request")]
    public string RequestId { get => this.requestId; set => this.requestId = value; }

    /// <summary>
    /// Gets or sets the credentials to use with this response.
    /// </summary>
    [JsonProperty("credentials", NullValueHandling = NullValueHandling.Ignore)]
    public AuthCredentials? Credentials { get => this.credentials; set => this.credentials = value; }

    /// <summary>
    /// Gets or sets the headers of the response.
    /// </summary>
    [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
    public List<Header>? Headers { get => this.headers; set => this.headers = value; }

    /// <summary>
    /// Gets or sets the cookies of the response.
    /// </summary>
    [JsonProperty("cookies", NullValueHandling = NullValueHandling.Ignore)]
    public List<SetCookieHeader>? Cookies { get => this.cookieHeaders; set => this.cookieHeaders = value; }

    /// <summary>
    /// Gets or sets the HTTP reason phrase ('OK', 'Not Found', etc.) of the response.
    /// </summary>
    [JsonProperty("reasonPhrase", NullValueHandling = NullValueHandling.Ignore)]
    public string? ReasonPhrase { get => this.reasonPhrase; set => this.reasonPhrase = value; }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    [JsonProperty("statusCode", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? StatusCode { get => this.statusCode; set => this.statusCode = value; }
}