// <copyright file="ShadowRootMode.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

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
    /// Shadow root is closed.
    /// </summary>
    Closed,
}
