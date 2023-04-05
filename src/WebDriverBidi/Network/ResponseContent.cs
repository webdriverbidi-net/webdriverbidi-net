// <copyright file="ResponseContent.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// Content of a response.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ResponseContent
{
    private ulong size = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseContent"/> class.
    /// </summary>
    internal ResponseContent()
    {
    }

    /// <summary>
    /// Gets the decoded size, in bytes, of the response body.
    /// </summary>
    [JsonProperty("size")]
    [JsonRequired]
    public ulong Size { get => this.size; internal set => this.size = value; }
}