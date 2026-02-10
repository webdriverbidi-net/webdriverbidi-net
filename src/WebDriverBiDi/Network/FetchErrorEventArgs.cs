// <copyright file="FetchErrorEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for events raised by before a network request is sent.
/// </summary>
public record FetchErrorEventArgs : BaseNetworkEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FetchErrorEventArgs"/> class.
    /// </summary>
    public FetchErrorEventArgs()
        : base()
    {
    }

    /// <summary>
    /// Gets the error text of the fetch error.
    /// </summary>
    [JsonPropertyName("errorText")]
    [JsonRequired]
    [JsonInclude]
    public string ErrorText { get; internal set; } = string.Empty;
}
