// <copyright file="WindowProxyProperties.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object representing the properties of a window proxy object.
/// </summary>
public record WindowProxyProperties
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowProxyProperties"/> class.
    /// </summary>
    [JsonConstructor]
    private WindowProxyProperties()
    {
    }

    /// <summary>
    /// Gets the browsing context ID for the window proxy.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    [JsonInclude]
    public string Context { get; internal set; } = string.Empty;
}
