// <copyright file="ResponseData.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Data of a network response.
/// </summary>
public record ResponseData
{
    private List<ReadOnlyHeader>? readOnlyHeaders;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseData"/> class.
    /// </summary>
    internal ResponseData()
    {
    }

    /// <summary>
    /// Gets the URL of the response.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the protocol of the response.
    /// </summary>
    [JsonPropertyName("protocol")]
    [JsonRequired]
    [JsonInclude]
    public string Protocol { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the status code of the response.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonRequired]
    [JsonInclude]
    public ulong Status { get; internal set; } = 0;

    /// <summary>
    /// Gets the status text of the response.
    /// </summary>
    [JsonPropertyName("statusText")]
    [JsonRequired]
    [JsonInclude]
    public string StatusText { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the response was retrieved from the cache.
    /// </summary>
    [JsonPropertyName("fromCache")]
    [JsonRequired]
    [JsonInclude]
    public bool FromCache { get; internal set; }

    /// <summary>
    /// Gets the headers of the response.
    /// </summary>
    [JsonIgnore]
    public IList<ReadOnlyHeader> Headers
    {
        get
        {
            this.readOnlyHeaders ??= [];
            foreach (Header header in this.SerializableHeaders)
            {
                this.readOnlyHeaders.Add(new ReadOnlyHeader(header));
            }

            return this.readOnlyHeaders.AsReadOnly();
        }
    }

    /// <summary>
    /// Gets the MIME type of the response.
    /// </summary>
    [JsonPropertyName("mimeType")]
    [JsonRequired]
    [JsonInclude]
    public string MimeType { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the count of the bytes received in the response.
    /// </summary>
    [JsonPropertyName("bytesReceived")]
    [JsonRequired]
    [JsonInclude]
    public ulong BytesReceived { get; internal set; } = 0;

    /// <summary>
    /// Gets the size, in bytes, of the headers in the response.
    /// </summary>
    [JsonPropertyName("headersSize")]
    [JsonRequired]
    [JsonInclude]
    public ulong? HeadersSize { get; internal set; }

    /// <summary>
    /// Gets the size, in bytes, of the body in the response.
    /// </summary>
    [JsonPropertyName("bodySize")]
    [JsonRequired]
    [JsonInclude]
    public ulong? BodySize { get; internal set; }

    /// <summary>
    /// Gets the size, in bytes, of the body in the response.
    /// </summary>
    [JsonPropertyName("content")]
    [JsonRequired]
    [JsonInclude]
    public ResponseContent Content { get; internal set; } = ResponseContent.Empty;

    /// <summary>
    /// Gets the list of authorization challenges in the response, if any.
    /// </summary>
    [JsonPropertyName("authChallenges")]
    [JsonInclude]
    public List<AuthChallenge>? AuthChallenges { get; internal set; }

    /// <summary>
    /// Gets or sets the headers of the response for serialization purposes.
    /// </summary>
    [JsonPropertyName("headers")]
    [JsonRequired]
    [JsonInclude]
    internal List<Header> SerializableHeaders { get; set; } = [];
}
