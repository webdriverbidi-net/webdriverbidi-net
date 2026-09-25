// <copyright file="RemoteGridOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Settings for the requests sent to a remote WebDriver grid, passed to
/// <see cref="BrowserLauncherBuilder.LaunchUsingRemoteGrid(Uri, RemoteGridOptions?)"/>. The session's
/// capabilities are set with <see cref="BrowserLauncherBuilder.WithSessionCapability(string, object?)"/>.
/// </summary>
public sealed class RemoteGridOptions
{
    /// <summary>
    /// Gets the HTTP headers sent with each request to the grid, such as an authorization token.
    /// Credentials in the grid URL (<c>https://user:key@host/</c>) are sent as Basic authorization.
    /// </summary>
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
}
