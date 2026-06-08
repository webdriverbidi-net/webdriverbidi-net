// <copyright file="NetworkRequestModification.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Network;

using WebDriverBiDi.Network;

/// <summary>
/// Describes modifications to apply to an intercepted network request whose URL matches a pattern.
/// </summary>
public class NetworkRequestModification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkRequestModification"/> class.
    /// </summary>
    /// <param name="urlPattern">A string URL pattern (passed as a <see cref="UrlPatternString"/>) that selects which requests this modification applies to.</param>
    public NetworkRequestModification(string urlPattern)
    {
        this.UrlPattern = urlPattern;
    }

    /// <summary>
    /// Gets the URL pattern string that selects which requests this modification applies to.
    /// </summary>
    public string UrlPattern { get; }

    /// <summary>
    /// Gets or sets the URL to substitute on the intercepted request, or <see langword="null"/> to leave unchanged.
    /// </summary>
    public string? ReplacementUrl { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method to substitute on the intercepted request, or <see langword="null"/> to leave unchanged.
    /// </summary>
    public string? ReplacementMethod { get; set; }

    /// <summary>
    /// Gets or sets the body to substitute on the intercepted request, or <see langword="null"/> to leave unchanged.
    /// The value is sent as a plain UTF-8 string.
    /// </summary>
    public string? ReplacementBody { get; set; }

    /// <summary>
    /// Gets the headers to add or overwrite on the intercepted request.
    /// </summary>
    public Dictionary<string, string> AdditionalHeaders { get; } = [];
}
