// <copyright file="AuthChallenge.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// A class providing credentials for authorization.
/// </summary>
public class AuthChallenge
{
    private string scheme = string.Empty;
    private string realm = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthChallenge"/> class.
    /// </summary>
    public AuthChallenge()
    {
    }

    /// <summary>
    /// Gets the scheme of the authentication challenge.
    /// </summary>
    [JsonPropertyName("scheme")]
    [JsonRequired]
    [JsonInclude]
    public string Scheme { get => this.scheme; internal set => this.scheme = value; }

    /// <summary>
    /// Gets the realm of the authentication challenge.
    /// </summary>
    [JsonPropertyName("realm")]
    [JsonRequired]
    [JsonInclude]
    public string Realm { get => this.realm; internal set => this.realm = value; }
}