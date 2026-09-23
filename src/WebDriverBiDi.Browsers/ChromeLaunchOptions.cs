// <copyright file="ChromeLaunchOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Launch settings that apply only to Chrome.
/// </summary>
public sealed class ChromeLaunchOptions : BrowserLaunchOptions
{
    /// <summary>
    /// Gets the browser to which these options apply, <see cref="BrowserKind.Chrome"/>.
    /// </summary>
    public override BrowserKind Browser => BrowserKind.Chrome;

    /// <summary>
    /// Gets or sets a value indicating whether to download and launch chrome-headless-shell, a smaller
    /// build of Chrome that always runs headless, in place of Chrome. It is available only from Chrome
    /// for Testing, so it cannot be used with the system-installed browser.
    /// </summary>
    public bool UseHeadlessShell { get; set; }
}
