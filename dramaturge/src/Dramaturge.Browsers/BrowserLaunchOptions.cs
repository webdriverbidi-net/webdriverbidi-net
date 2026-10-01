// <copyright file="BrowserLaunchOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

/// <summary>
/// Launch settings that apply to only one browser, passed to
/// <see cref="BrowserLauncherBuilder.WithBrowserOptions(BrowserLaunchOptions)"/>.
/// </summary>
public abstract class BrowserLaunchOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserLaunchOptions"/> class.
    /// </summary>
    private protected BrowserLaunchOptions()
    {
    }

    /// <summary>
    /// Gets the browser to which these options apply.
    /// </summary>
    public abstract BrowserKind Browser { get; }
}
