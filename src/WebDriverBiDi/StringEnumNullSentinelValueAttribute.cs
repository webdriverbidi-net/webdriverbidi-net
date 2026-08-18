// <copyright file="StringEnumNullSentinelValueAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

/// <summary>
/// Marks an enumerated type with string values with the enumerated value to
/// use to signal a null should be written to the JSON serialization.
/// </summary>
/// <typeparam name="T">The enumerated type to apply the null enumerated value to.</typeparam>
[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class StringEnumNullSentinelValueAttribute<T> : Attribute
    where T : struct, Enum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringEnumNullSentinelValueAttribute{T}"/> class.
    /// </summary>
    /// <param name="nullSentinelValue">
    /// The enumerated value to indicate a null value should be serialized.
    /// </param>
    public StringEnumNullSentinelValueAttribute(T nullSentinelValue)
    {
        this.NullSentinelValue = nullSentinelValue;
    }

    /// <summary>
    /// Gets the enumerated value to return if the string value does not match one of the enumerated options.
    /// </summary>
    public T NullSentinelValue { get; }
}
