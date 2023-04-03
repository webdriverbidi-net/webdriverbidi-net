// <copyright file="IncludeShadowTreeSerializationOption.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Values for the serialization of shadow trees when serializing nodes.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<IncludeShadowTreeSerializationOption>))]
public enum IncludeShadowTreeSerializationOption
{
    /// <summary>
    /// Do not include shadow trees when serializing nodes.
    /// </summary>
    None,

    /// <summary>
    /// Only include open shadow tress when serializing nodes.
    /// </summary>
    Open,

    /// <summary>
    /// Include all shadow trees, open or closed, when serializing nodes.
    /// </summary>
    All,
}