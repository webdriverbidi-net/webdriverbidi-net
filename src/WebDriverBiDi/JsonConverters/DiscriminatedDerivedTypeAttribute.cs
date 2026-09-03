// <copyright file="DiscriminatedDerivedTypeAttribute.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

/// <summary>
/// Describes a specific derived type of a base class to be deserialized based on
/// a value of a given field in the JSON payload.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class DiscriminatedDerivedTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiscriminatedDerivedTypeAttribute"/> class.
    /// </summary>
    /// <param name="derivedType">The derived type to deserialize when the discriminator matches.</param>
    /// <param name="typeDiscriminator">The discriminator string value that selects the derived type.</param>
    public DiscriminatedDerivedTypeAttribute(Type derivedType, string typeDiscriminator)
    {
        this.DerivedType = derivedType;
        this.Discriminator = typeDiscriminator;
    }

    /// <summary>
    /// Gets the derived type for the specified value.
    /// </summary>
    public Type DerivedType { get; }

    /// <summary>
    /// Gets the discriminator string value that selects <see cref="DerivedType"/> during deserialization.
    /// </summary>
    public string Discriminator { get; }
}
