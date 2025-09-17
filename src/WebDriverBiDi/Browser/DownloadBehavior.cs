// <copyright file="DownloadBehavior.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text.Json.Serialization;

/// <summary>
/// Class describing the download behavior, allowed or denied, to be used for the browser.
/// </summary>
[JsonDerivedType(typeof(DownloadBehaviorAllowed))]
[JsonDerivedType(typeof(DownloadBehaviorDenied))]
public class DownloadBehavior
{
    private DownloadBehaviorType type;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadBehavior"/> class.
    /// </summary>
    /// <param name="type">The <see cref="DownloadBehaviorType"/> to which to set the download behavior.</param>
    protected DownloadBehavior(DownloadBehaviorType type)
    {
        this.type = type;
    }

    /// <summary>
    /// Gets the type of download behavior, allowed or denied.
    /// </summary>
    [JsonPropertyName("type")]
    public DownloadBehaviorType Type => this.type;
}
