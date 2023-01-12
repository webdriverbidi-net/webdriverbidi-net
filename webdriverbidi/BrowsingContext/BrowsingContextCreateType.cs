// <copyright file="BrowsingContextCreateType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

/// <summary>
/// Values used for the creation of new browsing contexts.
/// </summary>
public enum BrowsingContextCreateType
{
    /// <summary>
    /// Create the browsing context in a new tab.
    /// </summary>
    Tab,

    /// <summary>
    /// Create the browsing context in a new window.
    /// </summary>
    Window,
}