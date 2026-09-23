// <copyright file="DownloadManifestException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Thrown when a download manifest is not valid, or does not list a requested build.
/// </summary>
internal sealed class DownloadManifestException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadManifestException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public DownloadManifestException(string message)
        : base(message)
    {
    }
}
