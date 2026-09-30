// <copyright file="BrowserCookie.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Network;

/// <summary>
/// A cookie of a <see cref="Browser"/>.
/// </summary>
/// <param name="Name">The cookie's name.</param>
/// <param name="Value">The cookie's value; a value the browser holds as bytes is read as UTF-8 text.</param>
/// <param name="Domain">The domain the cookie is sent to, such as <c>example.com</c>.</param>
public sealed record BrowserCookie(string Name, string Value, string Domain)
{
    /// <summary>
    /// Gets the path the cookie is sent for, or <see langword="null"/> for the browser's default when adding one.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Gets a value indicating whether the cookie is hidden from scripts.
    /// </summary>
    public bool HttpOnly { get; init; }

    /// <summary>
    /// Gets a value indicating whether the cookie is sent only over secure connections.
    /// </summary>
    public bool Secure { get; init; }

    /// <summary>
    /// Gets when the cookie is sent with requests from other sites, or <see langword="null"/> for the browser's
    /// default when adding one.
    /// </summary>
    public CookieSameSiteValue? SameSite { get; init; }

    /// <summary>
    /// Gets when the cookie expires, or <see langword="null"/> for a cookie that lasts for the session.
    /// </summary>
    public DateTime? Expires { get; init; }
}
