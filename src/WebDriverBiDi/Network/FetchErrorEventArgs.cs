// <copyright file="FetchErrorEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

/// <summary>
/// Object containing event data for events raised by before a network request is sent.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class FetchErrorEventArgs : BaseNetworkEventArgs
{
    private string errorText = string.Empty;

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
    [JsonProperty("errorText")]
    [JsonRequired]
    public string ErrorText { get => this.errorText; internal set => this.errorText = value; }
}