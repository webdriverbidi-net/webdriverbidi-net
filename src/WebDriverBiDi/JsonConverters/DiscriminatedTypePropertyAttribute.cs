// <copyright file="DiscriminatedTypePropertyAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

/// <summary>
/// Specifies that a base class used with <see cref="DiscriminatedUnionJsonConverter{T}"/>
/// uses value-based discriminator matching. The converter will inspect the value of the
/// named property in the JSON payload to determine which derived type to deserialize to
/// (e.g., <c>"type": "window"</c>).
/// </summary>
/// <remarks>
/// Apply this attribute to the base class along with one or more
/// <see cref="DiscriminatedDerivedTypeAttribute"/> attributes. To match derived types
/// by property presence rather than value, use <see cref="DiscriminatedTypePresenceAttribute"/>
/// instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DiscriminatedTypePropertyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiscriminatedTypePropertyAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the JSON property whose value identifies the derived type.</param>
    public DiscriminatedTypePropertyAttribute(string propertyName)
    {
        this.PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the name of the JSON property whose value identifies the derived type.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the type to deserialize to when the discriminator value does not match
    /// any registered derived type. When <see langword="null"/>, deserialization throws if
    /// no match is found.
    /// </summary>
    public Type? UnmatchedValueType { get; init; }

    /// <summary>
    /// Gets the behavior when the discriminator property is absent from the JSON
    /// payload. Defaults to <see cref="DiscriminatorPropertyMissingValueBehavior.ThrowException"/>.
    /// </summary>
    public DiscriminatorPropertyMissingValueBehavior PropertyMissingBehavior { get; init; } = DiscriminatorPropertyMissingValueBehavior.ThrowException;
}
