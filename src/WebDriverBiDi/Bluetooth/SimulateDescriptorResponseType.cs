// <copyright file="SimulateDescriptorResponseType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the simulation of descriptors.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<SimulateDescriptorResponseType>))]
public enum SimulateDescriptorResponseType
{
    /// <summary>
    /// Simulate a read descriptor response.
    /// </summary>
    Read,

    /// <summary>
    /// Simulate a write descriptor response.
    /// </summary>
    Write,
}
