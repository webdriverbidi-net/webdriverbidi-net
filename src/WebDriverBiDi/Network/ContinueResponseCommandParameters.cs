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
    /// <summary>
    /// Initializes a new instance of the <see cref="ContinueResponseCommandParameters" /> class.
    /// </summary>
    /// <param name="requestId">The ID of the request to continue.</param>
    public ContinueResponseCommandParameters(string requestId)
    {
        this.RequestId = requestId;
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
    public string RequestId { get; set; }

    /// <summary>
    /// Gets or sets the credentials to use with this response.
    /// </summary>
    [JsonPropertyName("credentials")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AuthCredentials? Credentials { get; set; }

    /// <summary>
    /// Gets or sets the headers of the response.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is one of the few list properties on a <see cref="CommandParameters"/> type that is nullable and
    /// settable, because the protocol gives a present-but-empty array its own meaning. The remote end steps
    /// for <c>network.continueResponse</c> state: "If command parameters contains "headers": Let headers be an empty
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
    /// Gets or sets the cookies of the response.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is one of the few list properties on a <see cref="CommandParameters"/> type that is nullable and
    /// settable, because the protocol gives a present-but-empty array its own meaning. In the remote end steps
    /// for <c>network.continueResponse</c>, when the command contains "cookies" the response's header list is
    /// rebuilt without its existing <c>Set-Cookie</c> headers and one <c>Set-Cookie</c> header is appended per
    /// supplied cookie, so sending <c>[]</c> replaces the response cookies with none, while omitting the field
    /// keeps the original cookies.
    /// </para>
    /// <para>
    /// When <see langword="null"/>, the property is not included in the command; when an empty list, an
    /// empty array is sent to the remote end. Every other list property is read-only and is omitted while empty.
    /// </para>
    /// </remarks>
    [JsonPropertyName("cookies")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SetCookieHeader>? Cookies { get; set; }

    /// <summary>
    /// Gets or sets the HTTP reason phrase ('OK', 'Not Found', etc.) of the response.
    /// </summary>
    [JsonPropertyName("reasonPhrase")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReasonPhrase { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    [JsonPropertyName("statusCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? StatusCode { get; set; }
}
