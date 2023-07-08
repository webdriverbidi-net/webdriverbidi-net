// <copyright file="AuthChallenge.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;

/// <summary>
/// A class providing credentials for authorization.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class AuthChallenge
{
    private string scheme = string.Empty;
    private string realm = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthChallenge"/> class.
    /// </summary>
    internal AuthChallenge()
    {
    }

    /// <summary>
    /// Gets the scheme of the authentication challenge.
    /// </summary>
    [JsonProperty("scheme")]
    [JsonRequired]
    public string Scheme { get => this.scheme; internal set => this.scheme = value; }

    /// <summary>
    /// Gets the realm of the authentication challenge.
    /// </summary>
    [JsonProperty("realm")]
    [JsonRequired]
    public string Realm { get => this.realm; internal set => this.realm = value; }
}