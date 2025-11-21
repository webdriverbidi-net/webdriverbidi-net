// <copyright file="PrintMarginParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Parameters of margins for printing.
/// </summary>
public class PrintMarginParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrintMarginParameters"/> class.
    /// </summary>
    public PrintMarginParameters()
    {
    }

    /// <summary>
    /// Gets or sets the left margin in centimeters for printing.
    /// The value must be greater than or equal to zero, and if omitted, defaults to 1.0.
    /// </summary>
    [JsonPropertyName("left")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Left
    {
        get;
        set
        {
            if (value is not null && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the right margin in centimeters for printing.
    /// The value must be greater than or equal to zero, and if omitted, defaults to 1.0.
    /// </summary>
    [JsonPropertyName("right")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Right
    {
        get;
        set
        {
            if (value is not null && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the top margin in centimeters for printing.
    /// The value must be greater than or equal to zero, and if omitted, defaults to 1.0.
    /// </summary>
    [JsonPropertyName("top")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Top
    {
        get;
        set
        {
            if (value is not null && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the bottom margin in centimeters for printing.
    /// The value must be greater than or equal to zero, and if omitted, defaults to 1.0.
    /// </summary>
    [JsonPropertyName("bottom")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Bottom
    {
        get;
        set
        {
            if (value is not null && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero");
            }

            field = value;
        }
    }
}
