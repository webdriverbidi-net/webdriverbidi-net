// <copyright file="DisplayModeMediaFeatureValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Provides values for the emulation of the "display-mode" CSS media feature.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<DisplayModeMediaFeatureValue>))]
[StringEnumNullSentinelValue<DisplayModeMediaFeatureValue>(Reset)]
public enum DisplayModeMediaFeatureValue
{
    /// <summary>
    /// The "fullscreen" value for the "display-mode" CSS media feature.
    /// </summary>
    Fullscreen,

    /// <summary>
    /// The "standalone" value for the "display-mode" CSS media feature.
    /// </summary>
    Standalone,

    /// <summary>
    /// The "minimal-ui" value for the "display-mode" CSS media feature.
    /// </summary>
    [StringEnumValue("minimal-ui")]
    MinimalUi,

    /// <summary>
    /// The "browser" value for the "display-mode" CSS media feature.
    /// </summary>
    Browser,

    /// <summary>
    /// The "picture-in-picture" value for the "display-mode" CSS media feature.
    /// </summary>
    [StringEnumValue("picture-in-picture")]
    PictureInPicture,

    /// <summary>
    /// A value to reset the emulation of the "display-mode" CSS media feature.
    /// </summary>
    Reset,
}
