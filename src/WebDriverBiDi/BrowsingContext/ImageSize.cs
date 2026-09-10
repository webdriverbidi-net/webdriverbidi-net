// <copyright file="ImageSize.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the image size of a captured screenshot.
/// </summary>
public class ImageSize
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSize"/> class.
    /// </summary>
    public ImageSize()
    {
    }

    /// <summary>
    /// Gets or sets the maximum width of the screenshot image.
    /// </summary>
    /// <remarks>
    /// The protocol requires this value to be greater than or equal to 1; a value of 0 is rejected by the remote end.
    /// </remarks>
    [JsonPropertyName("maxWidth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(1.0, double.PositiveInfinity)]
    public ulong? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets the maximum height of the screenshot image.
    /// </summary>
    /// <remarks>
    /// The protocol requires this value to be greater than or equal to 1; a value of 0 is rejected by the remote end.
    /// </remarks>
    [JsonPropertyName("maxHeight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(1.0, double.PositiveInfinity)]
    public ulong? MaxHeight { get; set; }
}
