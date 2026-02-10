// <copyright file="AuthChallenge.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// A class providing credentials for authorization.
/// </summary>
public record AuthChallenge
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthChallenge"/> class.
    /// </summary>
    [JsonConstructor]
    private AuthChallenge()
    {
    }

    /// <summary>
    /// Gets the scheme of the authentication challenge.
    /// </summary>
    [JsonPropertyName("scheme")]
    [JsonRequired]
    [JsonInclude]
    public string Scheme { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the realm of the authentication challenge.
    /// </summary>
    [JsonPropertyName("realm")]
    [JsonRequired]
    [JsonInclude]
    public string Realm { get; internal set; } = string.Empty;
}
