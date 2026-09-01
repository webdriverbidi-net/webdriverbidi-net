// <copyright file="ImageFormat.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the image format of a captured screenshot.
/// </summary>
public class ImageFormat
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageFormat"/> class.
    /// </summary>
    public ImageFormat()
    {
    }

    /// <summary>
    /// Gets or sets the MIME type of the image format. Defaults to "image/png".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "image/png";

    /// <summary>
    /// Gets or sets the quality of the image format.
    /// </summary>
    /// <remarks>
    /// Valid values for this property range from 0.0 to 1.0, inclusive. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("quality")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, 1.0)]
    public double? Quality { get; set; }
}
