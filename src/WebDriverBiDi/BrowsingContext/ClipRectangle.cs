// <copyright file="ClipRectangle.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

/// <summary>
/// The abstract base class for a clipping rectangle for a screenshot.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public abstract class ClipRectangle
{
    /// <summary>
    /// Gets the type of clip rectangle.
    /// </summary>
    [JsonProperty("type")]
    public abstract string Type { get; }
}