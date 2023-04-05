// <copyright file="ResponseCompletedEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// Object containing event data for events raised by before a network request is sent.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ResponseCompletedEventArgs : BaseNetworkEventArgs
{
    private ResponseData response = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseCompletedEventArgs"/> class.
    /// </summary>
    public ResponseCompletedEventArgs()
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