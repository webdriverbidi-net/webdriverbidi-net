// <copyright file="AnyPointerMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "any-pointer" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<AnyPointerMediaFeatureValue>))]
[StringEnumNullSentinelValue<AnyPointerMediaFeatureValue>(Reset)]
public enum AnyPointerMediaFeatureValue
{
    /// <summary>
    /// The "none" value for the "any-pointer" CSS media feature.
    /// </summary>
    None,

    /// <summary>
    /// The "coarse" value for the "any-pointer" CSS media feature.
    /// </summary>
    Coarse,

    /// <summary>
    /// The "fine" value for the "any-pointer" CSS media feature.
    /// </summary>
    Fine,

    /// <summary>
    /// A value to reset the emulation of the "any-pointer" CSS media feature.
    /// </summary>
    Reset,
}
