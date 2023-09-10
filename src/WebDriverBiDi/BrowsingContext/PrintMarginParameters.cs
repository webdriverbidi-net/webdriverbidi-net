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
    private double? bottom;
    private double? left;
    private double? right;
    private double? top;

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
        get
        {
            return this.left;
        }

        set
        {
            if (value is not null && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero");
            }

            this.left = value;
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
        get
        {
            return this.right;
        }

        set
        {
            if (value is not null && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero");
            }

            this.right = value;
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
        get
        {
            return this.top;
        }

        set
        {
            if (value is not null && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero");
            }

            this.top = value;
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
        get
        {
            return this.bottom;
        }

        set
        {
            if (value is not null && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than or equal to zero");
            }

            this.bottom = value;
        }
    }
}