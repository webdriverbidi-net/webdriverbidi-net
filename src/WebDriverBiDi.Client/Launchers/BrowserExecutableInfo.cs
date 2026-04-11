// <copyright file="BrowserExecutableInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Represents the location information for browser and optionally driver executables.
/// </summary>
public class BrowserExecutableInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserExecutableInfo"/> class.
    /// </summary>
    /// <param name="browserPath">The path to the browser executable.</param>
    /// <param name="driverPath">The path to the driver executable, or null if not included.</param>
    public BrowserExecutableInfo(string browserPath, string? driverPath = null)
    {
        this.BrowserPath = browserPath;
        this.DriverPath = driverPath;
    }

    /// <summary>
    /// Gets the path to the browser executable.
    /// </summary>
    public string BrowserPath { get; }

    /// <summary>
    /// Gets the path to the driver executable, or null if driver location was not requested.
    /// </summary>
    public string? DriverPath { get; }
}
