// <copyright file="ShadowRootMode.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Represents the mode of the shadow root for a node.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ShadowRootMode>))]
public enum ShadowRootMode
{
    /// <summary>
    /// Shadow root is open.
    /// </summary>
    Open,

    /// <summary>
    /// Shaodw root is closed.
    /// </summary>
    Closed,
}