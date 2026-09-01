// <copyright file="SpecRangeAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Marks a command-parameter property with the inclusive numeric range the WebDriver BiDi
/// specification defines for its value, so that Roslyn analyzers can read the range from compiled
/// metadata as well as from source.
/// </summary>
/// <remarks>
/// <para>
/// This attribute records the specification's range for documentation and tooling only. The library
/// deliberately does not validate a property's value against this range at run time: a value the
/// specification places outside the range is representable on the wire, and a conforming remote end
/// rejects it when the command is executed. Bounds are inclusive of both <see cref="Minimum"/> and
/// <see cref="Maximum"/>. Use <see cref="double.NegativeInfinity"/> or
/// <see cref="double.PositiveInfinity"/> for a range that is unbounded on that side.
/// </para>
/// <para>
/// Some properties accept a reset sentinel value that falls outside the specification range (for
/// example, a negative value that clears an override). Set <see cref="HasSentinel"/> to
/// <see langword="true"/> and <see cref="SentinelValue"/> to that value so tooling does not treat it
/// as out of range.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SpecRangeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpecRangeAttribute"/> class.
    /// </summary>
    /// <param name="minimum">The inclusive minimum value the specification allows, or <see cref="double.NegativeInfinity"/> if unbounded below.</param>
    /// <param name="maximum">The inclusive maximum value the specification allows, or <see cref="double.PositiveInfinity"/> if unbounded above.</param>
    public SpecRangeAttribute(double minimum, double maximum)
    {
        this.Minimum = minimum;
        this.Maximum = maximum;
    }

    /// <summary>
    /// Gets the inclusive minimum value the specification allows for the property.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the inclusive maximum value the specification allows for the property.
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the property accepts a reset sentinel value outside
    /// the specification range. When <see langword="true"/>, <see cref="SentinelValue"/> is that value.
    /// </summary>
    public bool HasSentinel { get; set; }

    /// <summary>
    /// Gets or sets the reset sentinel value the property accepts outside the specification range.
    /// This value is meaningful only when <see cref="HasSentinel"/> is <see langword="true"/>.
    /// </summary>
    public double SentinelValue { get; set; }
}
