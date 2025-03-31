// <copyright file="RequestData.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// A network request.
/// </summary>
public record RequestData
{
    private string requestId = string.Empty;
    private string url = string.Empty;
    private string method = string.Empty;
    private string destination = string.Empty;
    private string? initiatorType;
    private List<Header> headers = new();
    private List<ReadOnlyHeader>? readOnlyHeaders;
    private List<Cookie> cookies = new();
    private ulong? headersSize;
    private ulong? bodySize;
    private FetchTimingInfo timingInfo = FetchTimingInfo.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestData"/> class.
    /// </summary>
    internal RequestData()
    {
    }

    /// <summary>
    /// Gets the ID of the request.
    /// </summary>
    [JsonPropertyName("request")]
    [JsonRequired]
    [JsonInclude]
    public string RequestId { get => this.requestId; private set => this.requestId = value; }

    /// <summary>
    /// Gets the URL of the request.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get => this.url; private set => this.url = value; }

    /// <summary>
    /// Gets the method of the request.
    /// </summary>
    [JsonPropertyName("method")]
    [JsonRequired]
    [JsonInclude]
    public string Method { get => this.method; private set => this.method = value; }

    /// <summary>
    /// Gets the destination of the request.
    /// </summary>
    [JsonPropertyName("destination")]
    [JsonRequired]
    [JsonInclude]
    public string Destination { get => this.destination; private set => this.destination = value; }

    /// <summary>
    /// Gets the initiator type of the request.
    /// </summary>
    [JsonPropertyName("initiatorType")]
    [JsonRequired]
    [JsonInclude]
    public string? InitiatorType { get => this.initiatorType; private set => this.initiatorType = value; }

    /// <summary>
    /// Gets the headers of the request.
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
    /// Gets the cookies of the request.
    /// </summary>
    [JsonIgnore]
    public IList<Cookie> Cookies => this.cookies.AsReadOnly();

    /// <summary>
    /// Gets the size, in bytes, of the headers in the request.
    /// </summary>
    [JsonPropertyName("headersSize")]
    [JsonRequired]
    [JsonInclude]
    public ulong? HeadersSize { get => this.headersSize; private set => this.headersSize = value; }

    /// <summary>
    /// Gets the size, in bytes, of the body in the request.
    /// </summary>
    [JsonPropertyName("bodySize")]
    [JsonRequired]
    [JsonInclude]
    public ulong? BodySize { get => this.bodySize; private set => this.bodySize = value; }

    /// <summary>
    /// Gets the fetch timing info of the request.
    /// </summary>
    [JsonPropertyName("timings")]
    [JsonRequired]
    [JsonInclude]
    public FetchTimingInfo Timings { get => this.timingInfo; private set => this.timingInfo = value; }

    /// <summary>
    /// Gets or sets the headers of the request for serialization purposes.
    /// </summary>
    [JsonPropertyName("headers")]
    [JsonRequired]
    [JsonInclude]
    internal List<Header> SerializableHeaders { get => this.headers; set => this.headers = value; }

    /// <summary>
    /// Gets or sets the cookies of the request for serialization purposes.
    /// </summary>
    [JsonPropertyName("cookies")]
    [JsonRequired]
    [JsonInclude]
    internal List<Cookie> SerializableCookies { get => this.cookies; set => this.cookies = value; }
}
