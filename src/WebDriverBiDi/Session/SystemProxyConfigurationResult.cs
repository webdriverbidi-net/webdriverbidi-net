// <copyright file="SystemProxyConfigurationResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// Object representing a read only representation of the system proxy configuration used by the browser for this session.
/// </summary>
public record SystemProxyConfigurationResult : ProxyConfigurationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemProxyConfigurationResult"/> class.
    /// </summary>
    /// <param name="proxy">The system proxy configuration.</param>
    public SystemProxyConfigurationResult(SystemProxyConfiguration proxy)
        : base(proxy)
    {
    }
}
