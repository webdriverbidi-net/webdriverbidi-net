// <copyright file="SystemProxyConfiguration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// Object representing a system proxy to be used by the browser.
/// </summary>
public class SystemProxyConfiguration : ProxyConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SystemProxyConfiguration"/> class.
    /// </summary>
    public SystemProxyConfiguration()
        : base(ProxyType.System)
    {
    }
}
