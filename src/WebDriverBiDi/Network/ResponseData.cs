// <copyright file="ResponseData.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Data of a network response.
/// </summary>
public class ResponseData
{
    private string url = string.Empty;
    private string protocol = string.Empty;
    private ulong status = 0;
    private string statusText = string.Empty;
    private bool fromCache;
    private List<Header> headers = new();
    private List<ReadOnlyHeader>? readOnlyHeaders;
    private string mimeType = string.Empty;
    private ulong bytesReceived = 0;
    private ulong? headersSize;
    private ulong? bodySize;
    private ResponseContent content = new();
    private AuthChallenge? authChallenge;

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
    public string Url { get => this.url; internal set => this.url = value; }

    /// <summary>
    /// Gets the protocol of the response.
    /// </summary>
    [JsonPropertyName("protocol")]
    [JsonRequired]
    [JsonInclude]
    public string Protocol { get => this.protocol; internal set => this.protocol = value; }

    /// <summary>
    /// Gets the status code of the response.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonRequired]
    [JsonInclude]
    public ulong Status { get => this.status; internal set => this.status = value; }

    /// <summary>
    /// Gets the status text of the response.
    /// </summary>
    [JsonPropertyName("statusText")]
    [JsonRequired]
    [JsonInclude]
    public string StatusText { get => this.statusText; internal set => this.statusText = value; }

    /// <summary>
    /// Gets a value indicating whether the response was retrieved from the cache.
    /// </summary>
    [JsonPropertyName("fromCache")]
    [JsonRequired]
    [JsonInclude]
    public bool FromCache { get => this.fromCache; internal set => this.fromCache = value; }

    /// <summary>
    /// Gets the headers of the response.
    /// </summary>
    [JsonIgnore]
    public IList<ReadOnlyHeader> Headers
    {
        get
        {
            this.readOnlyHeaders ??= new();
            foreach (Header header in this.headers)
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
    public string MimeType { get => this.mimeType; internal set => this.mimeType = value; }

    /// <summary>
    /// Gets the count of the bytes received in the response.
    /// </summary>
    [JsonPropertyName("bytesReceived")]
    [JsonRequired]
    [JsonInclude]
    public ulong BytesReceived { get => this.bytesReceived; internal set => this.bytesReceived = value; }

    /// <summary>
    /// Gets the size, in bytes, of the headers in the response.
    /// </summary>
    [JsonPropertyName("headersSize")]
    [JsonRequired]
    [JsonInclude]
    public ulong? HeadersSize { get => this.headersSize; internal set => this.headersSize = value; }

    /// <summary>
    /// Gets the size, in bytes, of the body in the response.
    /// </summary>
    [JsonPropertyName("bodySize")]
    [JsonRequired]
    [JsonInclude]
    public ulong? BodySize { get => this.bodySize; internal set => this.bodySize = value; }

    /// <summary>
    /// Gets the size, in bytes, of the body in the response.
    /// </summary>
    [JsonPropertyName("content")]
    [JsonRequired]
    [JsonInclude]
    public ResponseContent Content { get => this.content; internal set => this.content = value; }

    /// <summary>
    /// Gets the authorization challenge in the response, if any.
    /// </summary>
    [JsonPropertyName("authChallenge")]
    [JsonInclude]
    public AuthChallenge? AuthChallenge { get => this.authChallenge; internal set => this.authChallenge = value; }

    /// <summary>
    /// Gets or sets the headers of the response for serialization purposes.
    /// </summary>
    [JsonPropertyName("headers")]
    [JsonRequired]
    [JsonInclude]
    internal List<Header> SerializableHeaders { get => this.headers; set => this.headers = value; }
}