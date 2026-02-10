// <copyright file="BeforeRequestSentEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing event data for events raised by before a network request is sent.
/// </summary>
public record BeforeRequestSentEventArgs : BaseNetworkEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BeforeRequestSentEventArgs"/> class.
    /// </summary>
    public BeforeRequestSentEventArgs()
        : base()
    {
    }

    /// <summary>
    /// Gets the initiator of the request.
    /// </summary>
    [JsonPropertyName("initiator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public Initiator? Initiator { get; internal set; }
}
