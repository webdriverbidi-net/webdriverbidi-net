// <copyright file="BrowserType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

/// <summary>
/// Enumerated value indicating the types of browsers supported.
/// </summary>
public enum BrowserType
{
    /// <summary>
    /// The Chrome browser, using only the browser executable.
    /// </summary>
    Chrome,

    /// <summary>
    /// The Firefox browser, using only the browser executable.
    /// </summary>
    Firefox,

    /// <summary>
    /// The Chrome browser, using pipe-based communication via --remote-debugging-pipe.
    /// This provides native BiDi support on Unix systems using file descriptors 3 and 4.
    /// </summary>
    ChromePipe,

    /// <summary>
    /// The Chrome browser, using the chromedriver executable.
    /// </summary>
    ChromeDriver,

    /// <summary>
    /// The Firefox browser, using the geckodriver executable.
    /// </summary>
    GeckoDriver,
}
