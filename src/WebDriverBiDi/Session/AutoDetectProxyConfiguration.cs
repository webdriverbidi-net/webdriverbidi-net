// <copyright file="AutoDetectProxyConfiguration.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

/// <summary>
/// Object representing an autodetect proxy to be used by the browser.
/// </summary>
public class AutoDetectProxyConfiguration : ProxyConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoDetectProxyConfiguration"/> class.
    /// </summary>
    public AutoDetectProxyConfiguration()
        : base(ProxyType.AutoDetect)
    {
    }
}
