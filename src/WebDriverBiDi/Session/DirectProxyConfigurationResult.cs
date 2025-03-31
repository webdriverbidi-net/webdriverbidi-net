// <copyright file="DirectProxyConfigurationResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// Object representing a read only direct proxy configuration used by the browser for this session.
/// </summary>
public record DirectProxyConfigurationResult : ProxyConfigurationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectProxyConfigurationResult"/> class.
    /// </summary>
    /// <param name="proxy">The direct proxy configuration.</param>
    public DirectProxyConfigurationResult(DirectProxyConfiguration proxy)
        : base(proxy)
    {
    }
}
