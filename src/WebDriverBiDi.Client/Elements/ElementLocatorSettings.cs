// <copyright file="ElementLocatorSettings.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Elements;

/// <summary>
/// Provides settings for element locator operations.
/// </summary>
public class ElementLocatorSettings
{
    /// <summary>
    /// Gets or sets the default timeout for element locator operations.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
