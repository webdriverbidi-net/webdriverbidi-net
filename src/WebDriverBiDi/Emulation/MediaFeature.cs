// <copyright file="MediaFeature.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// A data class representing a media feature override value.
/// </summary>
public class MediaFeature
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaFeature"/> class.
    /// </summary>
    /// <param name="name">The name of the media feature to override.</param>
    /// <param name="value">The value of the media feature to override.</param>
    public MediaFeature(string name, string value)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name), "name of media feature must not be null");
        }

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "value of media feature must not be null");
        }

        this.Name = name;
        this.Value = value;
    }

    /// <summary>
    /// Gets or sets the name of the media feature to override.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonInclude]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the media feature to override.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonInclude]
    public string Value { get; set; }
}
