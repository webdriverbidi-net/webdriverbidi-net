// <copyright file="HarNotFound.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// What a route that replays an HTTP Archive does with a request the archive has no entry for.
/// </summary>
public enum HarNotFound
{
    /// <summary>
    /// Fails the request, as a network error would, so that a replayed page never reaches the network unseen.
    /// </summary>
    Abort,

    /// <summary>
    /// Passes the request to the next route that matches it, or, if none decides it, sends it.
    /// </summary>
    Fallback,
}
