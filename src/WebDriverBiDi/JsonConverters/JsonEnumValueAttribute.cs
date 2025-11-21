// <copyright file="JsonEnumValueAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

/// <summary>
/// Marks a specific enumerated value with its string representation.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class JsonEnumValueAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonEnumValueAttribute"/> class.
    /// </summary>
    /// <param name="enumValue">The string representation of the enumerated value.</param>
    public JsonEnumValueAttribute(string enumValue)
    {
        this.Value = enumValue;
    }

    /// <summary>
    /// Gets the value to use in JSON serialization and deserialization of the enumerated value.
    /// </summary>
    public string Value { get; }
}
