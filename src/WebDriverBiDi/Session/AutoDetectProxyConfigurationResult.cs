// <copyright file="AutoDetectProxyConfigurationResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// Object representing a read only autodetect proxy configuration used by the browser for this session.
/// </summary>
public record AutoDetectProxyConfigurationResult : ProxyConfigurationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoDetectProxyConfigurationResult"/> class.
    /// </summary>
    /// <param name="proxy">The autodetect proxy configuration.</param>
    public AutoDetectProxyConfigurationResult(AutoDetectProxyConfiguration proxy)
        : base(proxy)
    {
    }
}
