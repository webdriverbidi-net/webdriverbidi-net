// <copyright file="ImageFormat.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// Represents the image format of a captured screenshot.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class ImageFormat
{
    private string type = "image/png";
    private double? quality;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageFormat"/> class.
    /// </summary>
    public ImageFormat()
    {
    }

    /// <summary>
    /// Gets or sets the MIME type of the image format. Default to "image/png".
    /// </summary>
    [JsonProperty("type")]
    public string Type { get => this.type; set => this.type = value; }

    /// <summary>
    /// Gets or sets the quality of the image format. If specified, must be between 0 and 1 inclusive.
    /// </summary>
    [JsonProperty("quality", NullValueHandling = NullValueHandling.Ignore)]
    public double? Quality
    {
        get
        {
            return this.quality;
        }

        set
        {
            if (value is not null && (value < 0 || value > 1))
            {
                throw new WebDriverBiDiException("Quality must be between 0 and 1 inclusive.");
            }

            this.quality = value;
        }
    }
}
