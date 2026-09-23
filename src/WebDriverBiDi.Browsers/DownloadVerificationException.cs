// <copyright file="DownloadVerificationException.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers;

/// <summary>
/// Thrown when a downloaded file does not match the checksum or size its publisher lists, or
/// the published checksum cannot be found.
/// </summary>
internal sealed class DownloadVerificationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadVerificationException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public DownloadVerificationException(string message)
        : base(message)
    {
    }
}
