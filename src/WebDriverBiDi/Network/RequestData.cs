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
    private List<ReadOnlyHeader>? readOnlyHeaders;

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
    public string RequestId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the URL of the request.
    /// </summary>
    [JsonPropertyName("url")]
    [JsonRequired]
    [JsonInclude]
    public string Url { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the method of the request.
    /// </summary>
    [JsonPropertyName("method")]
    [JsonRequired]
    [JsonInclude]
    public string Method { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the destination of the request.
    /// </summary>
    [JsonPropertyName("destination")]
    [JsonRequired]
    [JsonInclude]
    public string Destination { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the initiator type of the request.
    /// </summary>
    [JsonPropertyName("initiatorType")]
    [JsonRequired]
    [JsonInclude]
    public string? InitiatorType { get; internal set; }

    /// <summary>
    /// Gets the headers of the request.
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
    /// Gets the cookies of the request.
    /// </summary>
    [JsonIgnore]
    public IList<Cookie> Cookies => this.SerializableCookies.AsReadOnly();

    /// <summary>
    /// Gets the size, in bytes, of the headers in the request.
    /// </summary>
    [JsonPropertyName("headersSize")]
    [JsonRequired]
    [JsonInclude]
    public ulong? HeadersSize { get; internal set; }

    /// <summary>
    /// Gets the size, in bytes, of the body in the request.
    /// </summary>
    [JsonPropertyName("bodySize")]
    [JsonRequired]
    [JsonInclude]
    public ulong? BodySize { get; internal set; }

    /// <summary>
    /// Gets the fetch timing info of the request.
    /// </summary>
    [JsonPropertyName("timings")]
    [JsonRequired]
    [JsonInclude]
    public FetchTimingInfo Timings { get; internal set; } = FetchTimingInfo.Empty;

    /// <summary>
    /// Gets or sets the headers of the request for serialization purposes.
    /// </summary>
    [JsonPropertyName("headers")]
    [JsonRequired]
    [JsonInclude]
    internal List<Header> SerializableHeaders { get; set; } = [];

    /// <summary>
    /// Gets or sets the cookies of the request for serialization purposes.
    /// </summary>
    [JsonPropertyName("cookies")]
    [JsonRequired]
    [JsonInclude]
    internal List<Cookie> SerializableCookies { get; set; } = [];
}
