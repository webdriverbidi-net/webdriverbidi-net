// <copyright file="BrandVersion.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the overriding client hints for the browser.
/// </summary>
public class BrandVersion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrandVersion"/> class.
    /// </summary>
    /// <param name="brand">The name of the brand.</param>
    /// <param name="version">The version string.</param>
    public BrandVersion(string brand, string version)
    {
        this.Brand = brand;
        this.Version = version;
    }

    /// <summary>
    /// Gets or sets the brand name.
    /// </summary>
    [JsonPropertyName("brand")]
    [JsonInclude]
    [JsonRequired]
    public string Brand { get; set; }

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    [JsonPropertyName("version")]
    [JsonInclude]
    [JsonRequired]
    public string Version { get; set; }
}
