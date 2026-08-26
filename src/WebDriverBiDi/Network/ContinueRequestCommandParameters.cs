// <copyright file="ContinueRequestCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the network.continueRequest command.
/// </summary>
public class ContinueRequestCommandParameters : CommandParameters<ContinueRequestCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContinueRequestCommandParameters" /> class.
    /// </summary>
    /// <param name="requestId">The ID of the request to continue.</param>
    public ContinueRequestCommandParameters(string requestId)
    {
        this.RequestId = requestId;
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
    public string RequestId { get; set; }

    /// <summary>
    /// Gets or sets the body of the request.
    /// </summary>
    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public BytesValue? Body { get; set; }

    /// <summary>
    /// Gets or sets the headers of the request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is one of the few list properties on a <see cref="CommandParameters"/> type that is nullable and
    /// settable, because the protocol gives a present-but-empty array its own meaning. The remote end steps
    /// for <c>network.continueRequest</c> state: "If command parameters contains "headers": Let headers be an empty
    /// header list" and then append each entry, so sending <c>[]</c> replaces the header list with none, while
    /// omitting the field keeps the original header list.
    /// </para>
    /// <para>
    /// When <see langword="null"/>, the property is not included in the command; when an empty list, an
    /// empty array is sent to the remote end. Every other list property is read-only and is omitted while empty.
    /// </para>
    /// </remarks>
    [JsonPropertyName("headers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Header>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the cookie headers of the request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is one of the few list properties on a <see cref="CommandParameters"/> type that is nullable and
    /// settable, because the protocol gives a present-but-empty array its own meaning. The remote end steps
    /// for <c>network.continueRequest</c> state: "If command parameters contains "cookies": Let cookies be an empty
    /// cookie list" and then append each entry, so sending <c>[]</c> replaces the cookie list with none, while
    /// omitting the field keeps the original cookie list.
    /// </para>
    /// <para>
    /// When <see langword="null"/>, the property is not included in the command; when an empty list, an
    /// empty array is sent to the remote end. Every other list property is read-only and is omitted while empty.
    /// </para>
    /// </remarks>
    [JsonPropertyName("cookies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CookieHeader>? Cookies { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method of the request.
    /// </summary>
    [JsonPropertyName("method")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Method { get; set; }

    /// <summary>
    /// Gets or sets the URL of the request.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }
}
