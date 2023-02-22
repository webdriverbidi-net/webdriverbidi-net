// <copyright file="PrintOrientation.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Values used for page orientation for printing browsing contexts.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<PrintOrientation>))]
public enum PrintOrientation
{
    /// <summary>
    /// Create the browsing context in a new tab.
    /// </summary>
    Portrait,

    /// <summary>
    /// Create the browsing context in a new window.
    /// </summary>
    Landscape,
}