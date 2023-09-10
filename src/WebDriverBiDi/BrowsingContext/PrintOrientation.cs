// <copyright file="PrintOrientation.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

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