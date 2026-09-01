// <copyright file="DiscriminatorPropertyMissingValueBehavior.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

/// <summary>
/// Defines the behavior to use when deserializing a JSON payload into a base
/// class with multiple derived types based on the value of a specific property
/// in the JSON payload, and the named property is missing from the payload.
/// </summary>
public enum DiscriminatorPropertyMissingValueBehavior
{
    /// <summary>
    /// Throw an exception when the property is missing from the JSON payload.
    /// </summary>
    ThrowException,

    /// <summary>
    /// Return <see langword="null"/> when the property is missing from the JSON payload.
    /// </summary>
    ReturnNull,
}
