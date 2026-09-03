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
public record BaseNetworkEventArgs : WebDriverBiDiEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseNetworkEventArgs"/> class.
    /// </summary>
    [JsonConstructor]
    internal BaseNetworkEventArgs()
    {
        this.EpochTimestamp = 0;
    }

    /// <summary>
    /// Gets the ID of the browsing context initiating the request, or <see langword="null"/>
    /// when the request is not associated with a browsing context (for example, a request made
    /// by a service worker).
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string? BrowsingContextId { get; internal set; }

    /// <summary>
    /// Gets the ID of the user context associated with the request, if any.
    /// </summary>
    [JsonPropertyName("userContext")]
    [JsonInclude]
    public string? UserContextId { get; internal set; }

    /// <summary>
    /// Gets the ID of the navigation initiating the request, or <see langword="null"/> when the
    /// request is not associated with a navigation.
    /// </summary>
    [JsonPropertyName("navigation")]
    [JsonRequired]
    [JsonInclude]
    public string? NavigationId { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether this request is blocked by a network intercept.
    /// </summary>
    [JsonPropertyName("isBlocked")]
    [JsonRequired]
    [JsonInclude]
    public bool IsBlocked { get; internal set; } = false;

    /// <summary>
    /// Gets the count of redirects for the request.
    /// </summary>
    [JsonPropertyName("redirectCount")]
    [JsonRequired]
    [JsonInclude]
    public ulong RedirectCount { get; internal set; } = 0;

    /// <summary>
    /// Gets the request data of the request.
    /// </summary>
    [JsonPropertyName("request")]
    [JsonRequired]
    [JsonInclude]
    public RequestData Request { get; internal set; } = new();

    /// <summary>
    /// Gets the list of network intercepts for this request.
    /// </summary>
    [JsonIgnore]
    public IList<string>? Intercepts => this.SerializableIntercepts?.AsReadOnly();

    /// <summary>
    /// Gets the timestamp of the navigation in UTC.
    /// </summary>
    [JsonIgnore]
    public DateTime Timestamp { get; internal set; } = DateTimeUtilities.UnixEpoch;

    /// <summary>
    /// Gets the timestamp as the total number of milliseconds elapsed since the start of the Unix epoch (1 January 1970 12:00AM UTC).
    /// </summary>
    [JsonPropertyName("timestamp")]
    [JsonRequired]
    [JsonInclude]
    public ulong EpochTimestamp
    {
        get;
        internal set
        {
            field = value;
            this.Timestamp = DateTimeUtilities.FromUnixEpochMilliseconds(value);
        }
    }

    /// <summary>
    /// Gets or sets the list of intercepts for this request, if any.
    /// </summary>
    [JsonPropertyName("intercepts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<string>? SerializableIntercepts { get; set; }
}
