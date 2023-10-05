// <copyright file="ResponseStartedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

/// <summary>
/// Object containing event data for events raised by before a network request is sent.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ResponseStartedEventArgs : BaseNetworkEventArgs
{
    private ResponseData response = new();

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
    [JsonProperty("response")]
    [JsonRequired]
    public ResponseData Response { get => this.response; internal set => this.response = value; }
}
