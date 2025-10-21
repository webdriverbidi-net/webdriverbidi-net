// <copyright file="AuthRequiredEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for events raised by before a network request is sent.
/// </summary>
public record AuthRequiredEventArgs : BaseNetworkEventArgs
{
    private ResponseData response = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthRequiredEventArgs"/> class.
    /// </summary>
    public AuthRequiredEventArgs()
        : base()
    {
    }

    /// <summary>
    /// Gets the initiator of the request.
    /// </summary>
    [JsonPropertyName("response")]
    [JsonRequired]
    [JsonInclude]
    public ResponseData Response { get => this.response; private set => this.response = value; }
}
