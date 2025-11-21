// <copyright file="AuthCredentials.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// A class providing credentials for authorization.
/// </summary>
public class AuthCredentials
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthCredentials"/> class.
    /// </summary>
    public AuthCredentials()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthCredentials"/> class.
    /// </summary>
    /// <param name="userName">The user name for the credentials.</param>
    /// <param name="password">The password for the credentials.</param>
    public AuthCredentials(string userName, string password)
    {
        this.UserName = userName;
        this.Password = password;
    }

    /// <summary>
    /// Gets the type of credentials.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "password";

    /// <summary>
    /// Gets or sets the user name to use for authentication.
    /// </summary>
    [JsonPropertyName("username")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password used for authentication.
    /// </summary>
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
