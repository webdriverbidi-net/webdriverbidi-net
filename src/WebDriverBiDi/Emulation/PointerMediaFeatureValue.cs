// <copyright file="PointerMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "pointer" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<PointerMediaFeatureValue>))]
[StringEnumNullSentinelValue<PointerMediaFeatureValue>(Reset)]
public enum PointerMediaFeatureValue
{
    /// <summary>
    /// The "none" value for the "pointer" CSS media feature.
    /// </summary>
    None,

    /// <summary>
    /// The "coarse" value for the "pointer" CSS media feature.
    /// </summary>
    Coarse,

    /// <summary>
    /// The "fine" value for the "pointer" CSS media feature.
    /// </summary>
    Fine,

    /// <summary>
    /// A value to reset the emulation of the "pointer" CSS media feature.
    /// </summary>
    Reset,
}
