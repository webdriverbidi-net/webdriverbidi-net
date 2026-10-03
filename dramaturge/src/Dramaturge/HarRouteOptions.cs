// <copyright file="HarRouteOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.Network;

/// <summary>
/// Settings of a route that replays an HTTP Archive.
/// </summary>
public sealed class HarRouteOptions
{
    /// <summary>
    /// Gets what the route does with a request the archive has no entry for. The default is
    /// <see cref="HarNotFound.Abort"/>.
    /// </summary>
    public HarNotFound NotFound { get; init; } = HarNotFound.Abort;

    /// <summary>
    /// Gets a pattern the browser matches first, so that only requests it matches are stopped, or
    /// <see langword="null"/> to stop every request while the route exists.
    /// </summary>
    public UrlPattern? Filter { get; init; }
}
