// <copyright file="PageCapture.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Testing;

/// <summary>
/// A screenshot or video of a page saved for a failed test, or a failure to save or record one.
/// </summary>
/// <param name="Path">The file written, or to have been written.</param>
/// <param name="Contents">The file's contents, or <see langword="null"/> for a failure.</param>
/// <param name="MediaType">The file's media type, "image/png" or "video/webm".</param>
/// <param name="Failure">What failed, as reported on the test, or <see langword="null"/> if the file was saved.</param>
internal sealed record PageCapture(string Path, byte[]? Contents, string MediaType, string? Failure)
{
    /// <summary>
    /// The media type of a screenshot.
    /// </summary>
    public const string Screenshot = "image/png";

    /// <summary>
    /// The media type of a video.
    /// </summary>
    public const string Video = "video/webm";
}
