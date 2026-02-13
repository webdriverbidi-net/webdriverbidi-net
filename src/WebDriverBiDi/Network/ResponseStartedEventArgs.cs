// <copyright file="ResponseStartedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for events raised by before a network request is sent.
/// </summary>
public record ResponseStartedEventArgs : BaseNetworkEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseStartedEventArgs"/> class.
    /// </summary>
    public ResponseStartedEventArgs()
        : base()
    {
    }

    /// <summary>
    /// Gets the initiator of the request.
    /// </summary>
    [JsonPropertyName("response")]
    [JsonRequired]
    [JsonInclude]
    public ResponseData Response { get; internal set; } = new();
}
