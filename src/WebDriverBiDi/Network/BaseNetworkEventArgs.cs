// <copyright file="BaseNetworkEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

/// <summary>
/// The base properties of all events for network traffic.
/// </summary>
[JsonDerivedType(typeof(AuthRequiredEventArgs))]
[JsonDerivedType(typeof(BeforeRequestSentEventArgs))]
[JsonDerivedType(typeof(FetchErrorEventArgs))]
[JsonDerivedType(typeof(ResponseCompletedEventArgs))]
[JsonDerivedType(typeof(ResponseStartedEventArgs))]
public record BaseNetworkEventArgs : WebDriverBiDiEventArgs
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
    internal BaseNetworkEventArgs()
    {
    }

    /// <summary>
    /// Gets the ID of the browsing context initiating the request.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string? BrowsingContextId { get => this.browsingContextId; private set => this.browsingContextId = value; }

    /// <summary>
    /// Gets the ID of the navigation initiating the request.
    /// </summary>
    [JsonPropertyName("navigation")]
    [JsonRequired]
    [JsonInclude]
    public string? NavigationId { get => this.navigationId; private set => this.navigationId = value; }

    /// <summary>
    /// Gets a value indicating whether this request is blocked by a network intercept.
    /// </summary>
    [JsonPropertyName("isBlocked")]
    [JsonRequired]
    [JsonInclude]
    public bool IsBlocked { get => this.isBlocked; private set => this.isBlocked = value; }

    /// <summary>
    /// Gets the count of redirects for the request.
    /// </summary>
    [JsonPropertyName("redirectCount")]
    [JsonRequired]
    [JsonInclude]
    public ulong RedirectCount { get => this.redirectCount; private set => this.redirectCount = value; }

    /// <summary>
    /// Gets the request data of the request.
    /// </summary>
    [JsonPropertyName("request")]
    [JsonRequired]
    [JsonInclude]
    public RequestData Request { get => this.request; private set => this.request = value; }

    /// <summary>
    /// Gets the list of network intercepts for this request.
    /// </summary>
    [JsonIgnore]
    public IList<string>? Intercepts => this.intercepts?.AsReadOnly();

    /// <summary>
    /// Gets the timestamp of the navigation in UTC.
    /// </summary>
    [JsonIgnore]
    public DateTime Timestamp => this.timestamp;

    /// <summary>
    /// Gets the timestamp as the total number of milliseconds elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonPropertyName("timestamp")]
    [JsonRequired]
    [JsonInclude]
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
    [JsonPropertyName("intercepts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableIntercepts { get => this.intercepts; set => this.intercepts = value; }
}
