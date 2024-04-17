// <copyright file="DirectProxyConfiguration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// Object representing a direct connection proxy to be used by the browser.
/// </summary>
public class DirectProxyConfiguration : ProxyConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectProxyConfiguration"/> class.
    /// </summary>
    public DirectProxyConfiguration()
        : base(ProxyType.Direct)
    {
    }
}
