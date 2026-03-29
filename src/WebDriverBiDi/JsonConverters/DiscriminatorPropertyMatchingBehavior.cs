// <copyright file="DiscriminatorPropertyMatchingBehavior.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

/// <summary>
/// Describes the behavior to use when matching the discriminated type property from the JSON
/// payload. Can specify whether to match based on the presence of a property, or the value
/// of a property.
/// </summary>
internal enum DiscriminatorPropertyMatchingBehavior
{
    /// <summary>
    /// Match the JSON payload to a derived type based on the value of the
    /// discriminated type property.
    /// </summary>
    Value,

    /// <summary>
    /// Match the JSON payload to a derived type based on the presence of a
    /// property with a given name in the JSON payload.
    /// </summary>
    Presence,
}
