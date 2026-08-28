// <copyright file="OverflowBlockMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "overflow-block" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<OverflowBlockMediaFeatureValue>))]
[StringEnumNullSentinelValue<OverflowBlockMediaFeatureValue>(Reset)]
public enum OverflowBlockMediaFeatureValue
{
    /// <summary>
    /// The "none" value for the "overflow-block" CSS media feature.
    /// </summary>
    None,

    /// <summary>
    /// The "scroll" value for the "overflow-block" CSS media feature.
    /// </summary>
    Scroll,

    /// <summary>
    /// The "optional-paged" value for the "overflow-block" CSS media feature.
    /// </summary>
    [StringEnumValue("optional-paged")]
    OptionalPaged,

    /// <summary>
    /// The "paged" value for the "overflow-block" CSS media feature.
    /// </summary>
    Paged,

    /// <summary>
    /// A value to reset the emulation of the "overflow-block" CSS media feature.
    /// </summary>
    Reset,
}
