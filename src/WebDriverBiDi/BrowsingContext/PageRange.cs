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
    private readonly ulong numericValue;
    private readonly bool isString;

    private PageRange(string value)
    {
        this.stringValue = value;
        this.isString = true;
    }

    private PageRange(ulong value)
    {
        this.numericValue = value;
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
    /// Gets the value as a numeric integer.
    /// </summary>
    internal ulong NumericValue => this.numericValue;

    /// <summary>
    /// Operator converting a string to a PageRange.
    /// </summary>
    /// <param name="value">The string value of the page range.</param>
    public static implicit operator PageRange(string value) => new(value);

    /// <summary>
    /// Operator converting an unsigned long to a PageRange.
    /// </summary>
    /// <param name="value">The unsigned long value of the page range.</param>
    public static implicit operator PageRange(ulong value) => new(value);
}
