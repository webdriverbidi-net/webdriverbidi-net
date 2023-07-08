// <copyright file="RequestData.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// A network request.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class RequestData
{
    private string requestId = string.Empty;
    private string url = string.Empty;
    private string method = string.Empty;
    private List<Header> headers = new();
    private List<ReadOnlyHeader>? readOnlyHeaders;
    private List<Cookie> cookies = new();
    private ulong? headersSize;
    private ulong? bodySize;
    private FetchTimingInfo timingInfo = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestData"/> class.
    /// </summary>
    internal RequestData()
    {
    }

    /// <summary>
    /// Gets the ID of the request.
    /// </summary>
    [JsonProperty("request")]
    [JsonRequired]
    public string RequestId { get => this.requestId; internal set => this.requestId = value; }

    /// <summary>
    /// Gets the URL of the request.
    /// </summary>
    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }

    /// <summary>
    /// Gets the method of the request.
    /// </summary>
    [JsonProperty("method")]
    [JsonRequired]
    public string Method { get => this.method; internal set => this.method = value; }

    /// <summary>
    /// Gets the headers of the request.
    /// </summary>
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
    public IList<Cookie> Cookies => this.cookies.AsReadOnly();

    /// <summary>
    /// Gets the size, in bytes, of the headers in the request.
    /// </summary>
    [JsonProperty("headersSize")]
    [JsonRequired]
    public ulong? HeadersSize { get => this.headersSize; internal set => this.headersSize = value; }

    /// <summary>
    /// Gets the size, in bytes, of the body in the request.
    /// </summary>
    [JsonProperty("bodySize")]
    public ulong? BodySize { get => this.bodySize; internal set => this.bodySize = value; }

    /// <summary>
    /// Gets the fetch timing info of the request.
    /// </summary>
    [JsonProperty("timings")]
    [JsonRequired]
    public FetchTimingInfo Timings { get => this.timingInfo; internal set => this.timingInfo = value; }

    /// <summary>
    /// Gets or sets the headers of the request for serialization purposes.
    /// </summary>
    [JsonProperty("headers")]
    [JsonRequired]
    internal List<Header> SerializableHeaders { get => this.headers; set => this.headers = value; }

    /// <summary>
    /// Gets or sets the cookies of the request for serialization purposes.
    /// </summary>
    [JsonProperty("cookies")]
    [JsonRequired]
    internal List<Cookie> SerializableCookies { get => this.cookies; set => this.cookies = value; }
}