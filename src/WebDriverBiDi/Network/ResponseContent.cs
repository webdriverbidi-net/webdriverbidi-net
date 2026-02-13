// <copyright file="ResponseContent.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Content of a response.
/// </summary>
public record ResponseContent
{
    [JsonConstructor]
    internal ResponseContent()
    {
    }

    /// <summary>
    /// Gets the decoded size, in bytes, of the response body.
    /// </summary>
    [JsonPropertyName("size")]
    [JsonRequired]
    [JsonInclude]
    public ulong Size { get; internal set; } = 0;

    /// <summary>
    /// Gets an. empty <see cref="ResponseContent"/> object.
    /// </summary>
    public static ResponseContent Empty => new();
}
