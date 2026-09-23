// <copyright file="TransientDownloadException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Thrown when a download is interrupted or corrupted in transit, so that downloading it again may succeed.
/// </summary>
internal sealed class TransientDownloadException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransientDownloadException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The exception that interrupted the download, if any.</param>
    public TransientDownloadException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
