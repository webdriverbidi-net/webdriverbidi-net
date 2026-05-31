// <copyright file="PageRange.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

/// <summary>
/// Represents a page range for printing pages.
/// </summary>
public readonly struct PageRange
{
    private readonly string? stringValue;
    private readonly long intValue;
    private readonly bool isString;

    private PageRange(string value)
    {
        this.stringValue = value;
        this.isString = true;
    }

    private PageRange(long value)
    {
        this.intValue = value;
        this.isString = false;
    }

    /// <summary>
    /// Gets a value indicating whether the range is a string.
    /// </summary>
    internal bool IsString => this.isString;

    /// <summary>
    /// Gets the value as a string.
    /// </summary>
    internal string StringValue => this.stringValue!;

    /// <summary>
    /// Gets the value as an integer.
    /// </summary>
    internal long IntValue => this.intValue;

    /// <summary>
    /// Operator converting a string to a PageRange.
    /// </summary>
    /// <param name="value">The string value of the page range.</param>
    public static implicit operator PageRange(string value) => new(value);

    /// <summary>
    /// Operator converting a long to a PageRange.
    /// </summary>
    /// <param name="value">The long value of the page range.</param>
    public static implicit operator PageRange(long value) => new(value);

    /// <summary>
    /// Operator converting an int to a PageRange.
    /// </summary>
    /// <param name="value">The int value of the page range.</param>
    public static implicit operator PageRange(int value) => new((long)value);
}
