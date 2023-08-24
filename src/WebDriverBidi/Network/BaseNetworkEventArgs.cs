// <copyright file="BaseNetworkEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;
using WebDriverBidi.Internal;

/// <summary>
/// The base properties of all events for network traffic.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class BaseNetworkEventArgs : WebDriverBidiEventArgs
{
    private string? browsingContextId;
    private string? navigationId;
    private bool isBlocked = false;
    private ulong redirectCount = 0;
    private RequestData request = new();
    private ulong epochTimestamp = 0;
    private DateTime timestamp = DateTimeUtilities.UnixEpoch;
    private List<string>? intercepts;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseNetworkEventArgs"/> class.
    /// </summary>
    protected BaseNetworkEventArgs()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context initiating the request.
    /// </summary>
    [JsonProperty("context", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)]
    public string? BrowsingContextId { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the ID of the navigation initiating the request.
    /// </summary>
    [JsonProperty("navigation", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Include)]
    public string? NavigationId { get => this.navigationId; internal set => this.navigationId = value; }

    /// <summary>
    /// Gets a value indicating whether this request is blocked by a network intercept.
    /// </summary>
    [JsonProperty("isBlocked")]
    [JsonRequired]
    public bool IsBlocked { get => this.isBlocked; internal set => this.isBlocked = value; }

    /// <summary>
    /// Gets the count of redirects for the request.
    /// </summary>
    [JsonProperty("redirectCount")]
    [JsonRequired]
    public ulong RedirectCount { get => this.redirectCount; internal set => this.redirectCount = value; }

    /// <summary>
    /// Gets the request data of the request.
    /// </summary>
    [JsonProperty("request")]
    [JsonRequired]
    public RequestData Request { get => this.request; internal set => this.request = value; }

    /// <summary>
    /// Gets the list of network intercepts for this request.
    /// </summary>
    public IList<string>? Intercepts => this.intercepts?.AsReadOnly();

    /// <summary>
    /// Gets the timestamp of the navigation in UTC.
    /// </summary>
    public DateTime Timestamp => this.timestamp;

    /// <summary>
    /// Gets the timestamp as the total number of milliseconds elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonProperty("timestamp")]
    [JsonRequired]
    public ulong EpochTimestamp
    {
        get
        {
            return this.epochTimestamp;
        }

        private set
        {
            this.epochTimestamp = value;
            this.timestamp = DateTimeUtilities.UnixEpoch.AddMilliseconds(value);
        }
    }

    /// <summary>
    /// Gets or sets the list of intercepts for this request, if any.
    /// </summary>
    [JsonProperty("intercepts", NullValueHandling = NullValueHandling.Ignore)]
    internal List<string>? SerializableIntercepts { get => this.intercepts; set => this.intercepts = value; }
}