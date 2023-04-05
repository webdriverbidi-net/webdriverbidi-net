// <copyright file="ResponseData.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// Data of a network response.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ResponseData
{
    private string url = string.Empty;
    private string protocol = string.Empty;
    private ulong status = 0;
    private string statusText = string.Empty;
    private bool fromCache;
    private List<Header> headers = new();
    private string mimeType = string.Empty;
    private ulong bytesReceived = 0;
    private ulong? headersSize;
    private ulong? bodySize;
    private ResponseContent content = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseData"/> class.
    /// </summary>
    internal ResponseData()
    {
    }

    /// <summary>
    /// Gets the URL of the response.
    /// </summary>
    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }

    /// <summary>
    /// Gets the protocol of the response.
    /// </summary>
    [JsonProperty("protocol")]
    [JsonRequired]
    public string Protocol { get => this.protocol; internal set => this.protocol = value; }

    /// <summary>
    /// Gets the status code of the response.
    /// </summary>
    [JsonProperty("status")]
    [JsonRequired]
    public ulong Status { get => this.status; internal set => this.status = value; }

    /// <summary>
    /// Gets the status text of the response.
    /// </summary>
    [JsonProperty("statusText")]
    [JsonRequired]
    public string StatusText { get => this.statusText; internal set => this.statusText = value; }

    /// <summary>
    /// Gets a value indicating whether the response was retrieved from the cache.
    /// </summary>
    [JsonProperty("fromCache")]
    [JsonRequired]
    public bool FromCache { get => this.fromCache; internal set => this.fromCache = value; }

    /// <summary>
    /// Gets the headers of the response.
    /// </summary>
    public IList<Header> Headers => this.headers.AsReadOnly();

    /// <summary>
    /// Gets the MIME type of the response.
    /// </summary>
    [JsonProperty("mimeType")]
    [JsonRequired]
    public string MimeType { get => this.mimeType; internal set => this.mimeType = value; }

    /// <summary>
    /// Gets the count of the bytes received in the response.
    /// </summary>
    [JsonProperty("bytesReceived")]
    [JsonRequired]
    public ulong BytesReceived { get => this.bytesReceived; internal set => this.bytesReceived = value; }

    /// <summary>
    /// Gets the size, in bytes, of the headers in the response.
    /// </summary>
    [JsonProperty("headersSize")]
    public ulong? HeadersSize { get => this.headersSize; internal set => this.headersSize = value; }

    /// <summary>
    /// Gets the size, in bytes, of the body in the response.
    /// </summary>
    [JsonProperty("bodySize")]
    public ulong? BodySize { get => this.bodySize; internal set => this.bodySize = value; }

    /// <summary>
    /// Gets the size, in bytes, of the body in the response.
    /// </summary>
    [JsonProperty("content")]
    [JsonRequired]
    public ResponseContent Content { get => this.content; internal set => this.content = value; }

    /// <summary>
    /// Gets or sets the headers of the response for serialization purposes.
    /// </summary>
    [JsonProperty("headers")]
    [JsonRequired]
    internal List<Header> SerializableHeaders { get => this.headers; set => this.headers = value; }
}