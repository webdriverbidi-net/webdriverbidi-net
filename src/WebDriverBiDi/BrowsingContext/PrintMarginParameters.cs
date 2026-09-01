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
    /// Gets or sets the left margin in centimeters for printing. If omitted, defaults to 1.0.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to 0.0. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("left")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, double.PositiveInfinity)]
    public double? Left { get; set; }

    /// <summary>
    /// Gets or sets the right margin in centimeters for printing. If omitted, defaults to 1.0.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to 0.0. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("right")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, double.PositiveInfinity)]
    public double? Right { get; set; }

    /// <summary>
    /// Gets or sets the top margin in centimeters for printing. If omitted, defaults to 1.0.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to 0.0. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("top")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, double.PositiveInfinity)]
    public double? Top { get; set; }

    /// <summary>
    /// Gets or sets the bottom margin in centimeters for printing. If omitted, defaults to 1.0.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to 0.0. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("bottom")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, double.PositiveInfinity)]
    public double? Bottom { get; set; }
}
