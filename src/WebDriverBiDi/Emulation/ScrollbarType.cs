// <copyright file="ScrollbarType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated values for the scroll bar type of the emulated display.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ScrollbarType>))]
public enum ScrollbarType
{
    /// <summary>
    /// The emulated scroll bar type is always present.
    /// </summary>
    Classic,

    /// <summary>
    /// The emulated scroll bar type is only overlaid on the canvas when actively scrolling.
    /// </summary>
    Overlay,
}
