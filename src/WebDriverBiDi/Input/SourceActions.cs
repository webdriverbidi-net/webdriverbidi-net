// <copyright file="SourceActions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using Newtonsoft.Json;

/// <summary>
/// Base class for input actions.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public abstract class SourceActions
{
    private readonly string id = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the type of the source actions.
    /// </summary>
    [JsonProperty("type")]
    public abstract string Type { get; }

    /// <summary>
    /// Gets the ID of the device.
    /// </summary>
    [JsonProperty("id")]
    public string Id => this.id;
}