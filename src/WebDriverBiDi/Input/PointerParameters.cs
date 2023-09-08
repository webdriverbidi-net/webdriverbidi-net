// <copyright file="PointerParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using Newtonsoft.Json;

/// <summary>
/// Represents the parameters of a pointer device.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PointerParameters
{
    private PointerType? pointerType;

    /// <summary>
    /// Gets or sets the type of pointer device.
    /// </summary>
    [JsonProperty("pointerType", NullValueHandling = NullValueHandling.Ignore)]
    public PointerType? PointerType { get => this.pointerType; set => this.pointerType = value; }
}