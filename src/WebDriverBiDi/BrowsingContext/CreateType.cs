// <copyright file="CreateType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the creation of new browsing contexts.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<CreateType>))]
public enum CreateType
{
    /// <summary>
    /// Create the browsing context in a new tab.
    /// </summary>
    Tab,

    /// <summary>
    /// Create the browsing context in a new window.
    /// </summary>
    Window,
}