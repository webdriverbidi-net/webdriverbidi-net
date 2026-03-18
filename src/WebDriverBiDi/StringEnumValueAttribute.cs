// <copyright file="StringEnumValueAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Marks a specific enumerated value with its string representation.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class StringEnumValueAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringEnumValueAttribute"/> class.
    /// </summary>
    /// <param name="enumValue">The string representation of the enumerated value.</param>
    public StringEnumValueAttribute(string enumValue)
    {
        this.Value = enumValue;
    }

    /// <summary>
    /// Gets the value to use in converting the enumerated value to and from a string.
    /// </summary>
    public string Value { get; }
}
