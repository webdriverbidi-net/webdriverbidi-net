// <copyright file="FirefoxLaunchOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

/// <summary>
/// Launch settings that apply only to Firefox.
/// </summary>
public sealed class FirefoxLaunchOptions : BrowserLaunchOptions
{
    /// <summary>
    /// Gets the browser to which these options apply, <see cref="BrowserKind.Firefox"/>.
    /// </summary>
    public override BrowserKind Browser => BrowserKind.Firefox;

    /// <summary>
    /// Gets the Firefox preferences, which override the launcher's defaults. Each value must be a
    /// <see cref="string"/>, a <see cref="bool"/>, or an <see cref="int"/>, the types Firefox
    /// preferences can have; any other is rejected when the launcher is built.
    /// </summary>
    public IDictionary<string, object> Preferences { get; } = new Dictionary<string, object>();
}
