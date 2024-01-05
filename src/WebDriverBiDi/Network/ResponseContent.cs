// <copyright file="ResponseContent.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Content of a response.
/// </summary>
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
    [JsonPropertyName("size")]
    [JsonRequired]
    [JsonInclude]
    public ulong Size { get => this.size; private set => this.size = value; }
}
