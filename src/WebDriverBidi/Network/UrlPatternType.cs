// <copyright file="UrlPatternType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

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