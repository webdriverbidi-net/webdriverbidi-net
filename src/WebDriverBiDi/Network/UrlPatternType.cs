// <copyright file="UrlPatternType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated value of types for a UrlPattern.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<UrlPatternType>))]
public enum UrlPatternType
{
    /// <summary>
    /// The UrlPattern is defined by a string.
    /// </summary>
    String,

    /// <summary>
    /// The UrlPattern is defined by a pattern.
    /// </summary>
    Pattern,
}