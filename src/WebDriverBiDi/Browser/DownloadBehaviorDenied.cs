// <copyright file="DownloadBehaviorDenied.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

/// <summary>
/// Class describing the settings for denying download in the browser.
/// </summary>
public class DownloadBehaviorDenied : DownloadBehavior
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadBehaviorDenied"/> class.
    /// </summary>
    public DownloadBehaviorDenied()
        : base(DownloadBehaviorType.Denied)
    {
    }
}
