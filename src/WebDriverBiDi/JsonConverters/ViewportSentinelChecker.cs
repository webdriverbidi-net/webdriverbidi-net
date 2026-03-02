// <copyright file="ViewportSentinelChecker.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using WebDriverBiDi.BrowsingContext;

/// <summary>
/// Sentinel checker for <see cref="Viewport"/> that treats the
/// <see cref="Viewport.ResetToDefaultViewport"/> instance as the sentinel null value.
/// </summary>
public class ViewportSentinelChecker : SentinelValueChecker<Viewport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewportSentinelChecker"/> class.
    /// </summary>
    public ViewportSentinelChecker()
    {
    }

    /// <inheritdoc />
    public override bool IsSentinelValue(Viewport value) => value.IsResetViewport;
}
