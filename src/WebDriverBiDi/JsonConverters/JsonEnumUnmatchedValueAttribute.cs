// <copyright file="JsonEnumUnmatchedValueAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

/// <summary>
/// Marks an enumerated type deserializable from JSON with the enumerated value to
/// be used if the serialized value does not match one of the enumerated values.
/// This attribute should be limited to temporary use, only to work around a
/// remote-end protocol implementation bug shipped by a vendor.
/// </summary>
/// <typeparam name="T">The enumerated type to apply the default deserialization value to.</typeparam>
[AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
public sealed class JsonEnumUnmatchedValueAttribute<T> : Attribute
    where T : struct, Enum
{
    private readonly T unmatchedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonEnumUnmatchedValueAttribute{T}"/> class.
    /// </summary>
    /// <param name="unmatchedValue">
    /// The enumerated value to return if the serialized JSON value
    /// does not match one of the enumerated options.
    /// </param>
    public JsonEnumUnmatchedValueAttribute(T unmatchedValue)
    {
        this.unmatchedValue = unmatchedValue;
    }

    /// <summary>
    /// Gets the enumerated value to return if the serialized JSON value does not match one of the enumerated options.
    /// </summary>
    public T UnmatchedValue => this.unmatchedValue;
}
