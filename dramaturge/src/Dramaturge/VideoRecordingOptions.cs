// <copyright file="VideoRecordingOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// Settings of a page's video recording. A size or frame rate left out is the browser's choice; the browser keeps
/// the page's proportions within the size.
/// </summary>
public sealed class VideoRecordingOptions
{
    /// <summary>
    /// Gets the video's largest width, in pixels.
    /// </summary>
    public ulong? Width { get; init; }

    /// <summary>
    /// Gets the video's largest height, in pixels.
    /// </summary>
    public ulong? Height { get; init; }

    /// <summary>
    /// Gets the video's frames per second.
    /// </summary>
    public ulong? FrameRate { get; init; }
}
