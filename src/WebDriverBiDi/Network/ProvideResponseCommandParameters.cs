// <copyright file="ProvideResponseCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the network.provideResponse command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ProvideResponseCommandParameters : CommandParameters<EmptyResult>
{
    private string requestId;
    private BytesValue? body;
    private List<SetCookieHeader>? cookieHeaders;
    private List<Header>? headers;
    private string? reasonPhrase;
    private uint? statusCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProvideResponseCommandParameters" /> class.
    /// </summary>
    /// <param name="requestId">The ID of the request to continue.</param>
    public ProvideResponseCommandParameters(string requestId)
    {
        this.requestId = requestId;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "network.provideResponse";

    /// <summary>
    /// Gets or sets the ID of the request to continue.
    /// </summary>
    [JsonProperty("request")]
    public string RequestId { get => this.requestId; set => this.requestId = value; }

    /// <summary>
    /// Gets or sets the body of the response.
    /// </summary>
    [JsonProperty("body", NullValueHandling = NullValueHandling.Ignore)]
    public BytesValue? Body { get => this.body; set => this.body = value; }

    /// <summary>
    /// Gets or sets the cookies of the response.
    /// </summary>
    [JsonProperty("cookies", NullValueHandling = NullValueHandling.Ignore)]
    public List<SetCookieHeader>? Cookies { get => this.cookieHeaders; set => this.cookieHeaders = value; }

    /// <summary>
    /// Gets or sets the headers of the response.
    /// </summary>
    [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
    public List<Header>? Headers { get => this.headers; set => this.headers = value; }

    /// <summary>
    /// Gets or sets the HTTP reason phrase ('OK', 'Not Found', etc.) of the response.
    /// </summary>
    [JsonProperty("reasonPhrase", NullValueHandling = NullValueHandling.Ignore)]
    public string? ReasonPhrase { get => this.reasonPhrase; set => this.reasonPhrase = value; }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    [JsonProperty("statusCode", NullValueHandling = NullValueHandling.Ignore)]
    public uint? StatusCode { get => this.statusCode; set => this.statusCode = value; }
}
