// <copyright file="RouteOverrides.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// Changes to make to a routed request as it continues. A change left <see langword="null"/> leaves the request
/// as it was.
/// </summary>
public sealed class RouteOverrides
{
    /// <summary>
    /// Gets the URL to send the request to instead.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Gets the method to send the request with instead, such as "POST".
    /// </summary>
    public string? Method { get; init; }

    /// <summary>
    /// Gets the headers to send instead of the request's own.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// Gets the body to send instead of the request's own.
    /// </summary>
    public string? Body { get; init; }
}
