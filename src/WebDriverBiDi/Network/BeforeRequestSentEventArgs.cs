// <copyright file="BeforeRequestSentEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

/// <summary>
/// Object containing event data for events raised by before a network request is sent.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class BeforeRequestSentEventArgs : BaseNetworkEventArgs
{
    private Initiator initiator = new();

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
    [JsonProperty("initiator")]
    [JsonRequired]
    public Initiator Initiator { get => this.initiator; internal set => this.initiator = value; }
}
