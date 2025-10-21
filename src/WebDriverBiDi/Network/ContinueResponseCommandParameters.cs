// <copyright file="ContinueResponseCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.continueResponse command.
/// </summary>
public class ContinueResponseCommandParameters : CommandParameters<ContinueResponseCommandResult>
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
    [JsonIgnore]
    public override string MethodName => "network.continueResponse";

    /// <summary>
    /// Gets or sets the ID of the request to continue.
    /// </summary>
    [JsonPropertyName("request")]
    public string RequestId { get => this.requestId; set => this.requestId = value; }

    /// <summary>
    /// Gets or sets the credentials to use with this response.
    /// </summary>
    [JsonPropertyName("credentials")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AuthCredentials? Credentials { get => this.credentials; set => this.credentials = value; }

    /// <summary>
    /// Gets or sets the headers of the response.
    /// </summary>
    [JsonPropertyName("headers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Header>? Headers { get => this.headers; set => this.headers = value; }

    /// <summary>
    /// Gets or sets the cookies of the response.
    /// </summary>
    [JsonPropertyName("cookies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SetCookieHeader>? Cookies { get => this.cookieHeaders; set => this.cookieHeaders = value; }

    /// <summary>
    /// Gets or sets the HTTP reason phrase ('OK', 'Not Found', etc.) of the response.
    /// </summary>
    [JsonPropertyName("reasonPhrase")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReasonPhrase { get => this.reasonPhrase; set => this.reasonPhrase = value; }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    [JsonPropertyName("statusCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? StatusCode { get => this.statusCode; set => this.statusCode = value; }
}
