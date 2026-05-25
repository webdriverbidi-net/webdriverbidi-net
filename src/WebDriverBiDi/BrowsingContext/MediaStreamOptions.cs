// <copyright file="MediaStreamOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Describes the media stream options for a screencast.
/// </summary>
public class MediaStreamOptions
{
    /// <summary>
    /// Gets or sets the video constraints for a screencast.
    /// </summary>
    [JsonPropertyName("video")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MediaTrackConstraints? Video { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the screencast should include an audio track. If omitted, defaults to false.
    /// </summary>
    [JsonPropertyName("audio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Audio { get; set; }
}
