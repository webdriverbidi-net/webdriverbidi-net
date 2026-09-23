// <copyright file="RemoteGridOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Settings for a session on a remote WebDriver grid, passed to
/// <see cref="BrowserLauncherBuilder.LaunchUsingRemoteGrid(Uri, RemoteGridOptions?)"/>.
/// </summary>
public sealed class RemoteGridOptions
{
    /// <summary>
    /// Gets the capabilities added to the new session request, such as <c>browserVersion</c>,
    /// <c>platformName</c>, or a vendor's options. Each value must be <see langword="null"/>, a
    /// <see cref="string"/>, a <see cref="bool"/>, a number, a dictionary with string keys whose
    /// values follow these rules, or a sequence of such values; any other is rejected when the
    /// launcher is built. The launcher sets <c>browserName</c> and <c>webSocketUrl</c> itself, and
    /// they cannot be replaced.
    /// </summary>
    public IDictionary<string, object?> Capabilities { get; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets the HTTP headers sent with each request to the grid, such as an authorization token.
    /// Credentials in the grid URL (<c>https://user:key@host/</c>) are sent as Basic authorization.
    /// </summary>
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
}
