// <copyright file="ScreenOrientationType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated values for the current screen orientation of the emulated display.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ScreenOrientationType>))]
public enum ScreenOrientationType
{
    /// <summary>
    /// The emulated screen orientation is of a screen whose primary orientation is portrait.
    /// </summary>
    [JsonEnumValue("portrait-primary")]
    PortraitPrimary,

    /// <summary>
    /// The emulated screen orientation is of a screen whose secondary orientation is portrait.
    /// </summary>
    [JsonEnumValue("portrait-secondary")]
    PortraitSecondary,

    /// <summary>
    /// The emulated screen orientation is of a screen whose primary orientation is portrait.
    /// </summary>
    [JsonEnumValue("landscape-primary")]
    LandscapePrimary,

    /// <summary>
    /// The emulated screen orientation is of a screen whose secondary orientation is portrait.
    /// </summary>
    [JsonEnumValue("landscape-secondary")]
    LandscapeSecondary,
}
