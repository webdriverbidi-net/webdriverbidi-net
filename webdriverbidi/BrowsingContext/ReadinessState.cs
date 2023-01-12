// <copyright file="ReadinessState.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

/// <summary>
/// The readiness state of the browsing context.
/// </summary>
public enum ReadinessState
{
    /// <summary>
    /// Return immediately without checking for readiness state.
    /// </summary>
    None,

    /// <summary>
    /// Return after the readiness state becomes "interactive".
    /// </summary>
    Interactive,

    /// <summary>
    /// Return after the readiness state becomes "complete".
    /// </summary>
    Complete,
}