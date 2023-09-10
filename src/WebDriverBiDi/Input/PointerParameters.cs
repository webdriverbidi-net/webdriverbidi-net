// <copyright file="PointerParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the parameters of a pointer device.
/// </summary>
public class PointerParameters
{
    private PointerType? pointerType;

    /// <summary>
    /// Gets or sets the type of pointer device.
    /// </summary>
    [JsonPropertyName("pointerType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PointerType? PointerType { get => this.pointerType; set => this.pointerType = value; }
}