// <copyright file="InnerTextMatchType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the creation of new browsing contexts.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<InnerTextMatchType>))]
public enum InnerTextMatchType
{
    /// <summary>
    /// Locator matches a substring of the inner text.
    /// </summary>
    Partial,

    /// <summary>
    /// Locator matches the full value of the inner text.
    /// </summary>
    Full,
}
