// <copyright file="PrintPageParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Parameters of page size for printing.
/// </summary>
public class PrintPageParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrintPageParameters"/> class.
    /// </summary>
    public PrintPageParameters()
    {
    }

    /// <summary>
    /// Gets or sets the width in centimeters of the page for printing. If omitted, defaults to 21.59.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to 0.0352 (the protocol's minimum of
    /// one point). This property does not validate its value; a value outside this range is sent
    /// as-is, and a conforming remote end rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    [SpecRange(0.0352, double.PositiveInfinity)]
    public double? Width { get; set; }

    /// <summary>
    /// Gets or sets the height in centimeters of the page for printing. If omitted, defaults to 27.94.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to 0.0352 (the protocol's minimum of
    /// one point). This property does not validate its value; a value outside this range is sent
    /// as-is, and a conforming remote end rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    [SpecRange(0.0352, double.PositiveInfinity)]
    public double? Height { get; set; }
}
