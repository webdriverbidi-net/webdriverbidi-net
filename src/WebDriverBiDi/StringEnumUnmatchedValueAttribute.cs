// <copyright file="StringEnumUnmatchedValueAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Marks an enumerated type with string values with the enumerated value to
/// be used if the serialized value does not match one of the enumerated values.
/// </summary>
/// <typeparam name="T">The enumerated type to apply the default enumerated value to.</typeparam>
[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class StringEnumUnmatchedValueAttribute<T> : Attribute
    where T : struct, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringEnumUnmatchedValueAttribute{T}"/> class.
    /// </summary>
    /// <param name="unmatchedValue">
    /// The enumerated value to return if a string value does not match one of the
    /// enumerated options.
    /// </param>
    public StringEnumUnmatchedValueAttribute(T unmatchedValue)
    {
        this.UnmatchedValue = unmatchedValue;
    }

    /// <summary>
    /// Gets the enumerated value to return if the string value does not match one of the enumerated options.
    /// </summary>
    public T UnmatchedValue { get; }
}
