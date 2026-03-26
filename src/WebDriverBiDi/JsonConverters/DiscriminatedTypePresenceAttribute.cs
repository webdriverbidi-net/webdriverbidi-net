// <copyright file="DiscriminatedTypePresenceAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

/// <summary>
/// Specifies that a base class used with <see cref="DiscriminatedUnionJsonConverter{T}"/>
/// uses presence-based discriminator matching. The converter will determine the derived
/// type by which property name from the registered set is present in the JSON payload
/// (e.g., the presence of a <c>"context"</c> property identifies one derived type while
/// the presence of a <c>"realm"</c> property identifies another).
/// </summary>
/// <remarks>
/// Apply this attribute to the base class along with one or more
/// <see cref="DiscriminatedDerivedTypeAttribute"/> attributes, where each discriminator
/// string is the JSON property name whose presence identifies that derived type. To match
/// derived types by the value of a specific property, use
/// <see cref="DiscriminatedTypePropertyAttribute"/> instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class DiscriminatedTypePresenceAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiscriminatedTypePresenceAttribute"/> class.
    /// </summary>
    public DiscriminatedTypePresenceAttribute()
    {
    }

    /// <summary>
    /// Gets the behavior when no registered property name is present in the JSON
    /// payload. Defaults to <see cref="DiscriminatorPropertyMissingValueBehavior.ThrowException"/>.
    /// </summary>
    public DiscriminatorPropertyMissingValueBehavior PropertyMissingBehavior { get; init; } = DiscriminatorPropertyMissingValueBehavior.ThrowException;
}
