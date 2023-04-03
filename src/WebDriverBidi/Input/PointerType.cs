// <copyright file="PointerType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Input;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Enumeration of the types of pointer devices.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<PointerType>))]
public enum PointerType
{
    /// <summary>
    /// Pointer device is a mouse.
    /// </summary>
    Mouse,

    /// <summary>
    /// Pointer device is a pen-like stylus device.
    /// </summary>
    Pen,

    /// <summary>
    /// Pointer device is a touch device.
    /// </summary>
    Touch,
}