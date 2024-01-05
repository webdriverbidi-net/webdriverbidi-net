// <copyright file="ContinueRequestCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.continueRequest command.
/// </summary>
public class ContinueRequestCommandParameters : CommandParameters<EmptyResult>
{
    private string requestId;
    private BytesValue? body;
    private List<CookieHeader>? cookieHeaders;
    private List<Header>? headers;
    private string? method;
    private string? url;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContinueRequestCommandParameters" /> class.
    /// </summary>
    /// <param name="requestId">The ID of the request to continue.</param>
    public ContinueRequestCommandParameters(string requestId)
    {
        this.requestId = requestId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "network.continueRequest";

    /// <summary>
    /// Gets or sets the ID of the request to continue.
    /// </summary>
    [JsonPropertyName("request")]
    public string RequestId { get => this.requestId; set => this.requestId = value; }

    /// <summary>
    /// Gets or sets the body of the request.
    /// </summary>
    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public BytesValue? Body { get => this.body; set => this.body = value; }

    /// <summary>
    /// Gets or sets the headers of the request.
    /// </summary>
    [JsonPropertyName("headers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Header>? Headers { get => this.headers; set => this.headers = value; }

    /// <summary>
    /// Gets or sets the cookie headers of the request.
    /// </summary>
    [JsonPropertyName("cookies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CookieHeader>? Cookies { get => this.cookieHeaders; set => this.cookieHeaders = value; }

    /// <summary>
    /// Gets or sets the HTTP method of the request.
    /// </summary>
    [JsonPropertyName("method")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Method { get => this.method; set => this.method = value; }

    /// <summary>
    /// Gets or sets the URL of the request.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get => this.url; set => this.url = value; }
}
