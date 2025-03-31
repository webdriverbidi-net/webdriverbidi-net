// <copyright file="ResponseContent.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;

/// <summary>
/// Content of a response.
/// </summary>
public record ResponseContent
{
    private ulong size = 0;

    [JsonConstructor]
    private ResponseContent()
    {
    }

    /// <summary>
    /// Gets the decoded size, in bytes, of the response body.
    /// </summary>
    [JsonPropertyName("size")]
    [JsonRequired]
    [JsonInclude]
    public ulong Size { get => this.size; private set => this.size = value; }

    /// <summary>
    /// Gets an. empty <see cref="ResponseContent"/> object.
    /// </summary>
    public static ResponseContent Empty => new();
}
