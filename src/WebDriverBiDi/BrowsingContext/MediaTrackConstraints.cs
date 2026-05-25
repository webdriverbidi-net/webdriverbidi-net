// <copyright file="MediaTrackConstraints.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Describes the media track constraints for a screencast video stream.
/// </summary>
public class MediaTrackConstraints
{
    /// <summary>
    /// Gets or sets the width of the screencast video stream.
    /// </summary>
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the screencast video stream.
    /// </summary>
    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Height { get; set; }

    /// <summary>
    /// Gets or sets the frame rate of the screencast video stream.
    /// </summary>
    [JsonPropertyName("frameRate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? FrameRate { get; set; }
}
