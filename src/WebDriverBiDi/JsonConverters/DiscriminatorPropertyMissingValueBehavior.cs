// <copyright file="DiscriminatorPropertyMissingValueBehavior.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

/// <summary>
/// Defines the behavior to use when deserializing a JSON payload into a base
/// class with multiple derived types based on the value of a specific property
/// in the JSON payload, when the value of that property does not match any
/// known derived types.
/// </summary>
public enum DiscriminatorPropertyMissingValueBehavior
{
    /// <summary>
    /// Throw an exception when the value of the type property does not match any known derived types.
    /// </summary>
    ThrowException,

    /// <summary>
    /// Use the base type when the value of the type property does not match any known derived types.
    /// </summary>
    ReturnNull,
}
