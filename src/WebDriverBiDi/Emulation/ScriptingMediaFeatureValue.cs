// <copyright file="ScriptingMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "scripting" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ScriptingMediaFeatureValue>))]
[StringEnumNullSentinelValue<ScriptingMediaFeatureValue>(Reset)]
public enum ScriptingMediaFeatureValue
{
    /// <summary>
    /// The "none" value for the "scripting" CSS media feature.
    /// </summary>
    None,

    /// <summary>
    /// The "initial-only" value for the "scripting" CSS media feature.
    /// </summary>
    [StringEnumValue("initial-only")]
    InitialOnly,

    /// <summary>
    /// The "enabled" value for the "scripting" CSS media feature.
    /// </summary>
    Enabled,

    /// <summary>
    /// A value to reset the emulation of the "scripting" CSS media feature.
    /// </summary>
    Reset,
}
