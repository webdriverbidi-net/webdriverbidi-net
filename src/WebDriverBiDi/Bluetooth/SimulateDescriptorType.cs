// <copyright file="SimulateDescriptorType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the simulation of descriptors.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<SimulateDescriptorType>))]
public enum SimulateDescriptorType
{
    /// <summary>
    /// Simulate adding a descriptor.
    /// </summary>
    Add,

    /// <summary>
    /// Simulate removing a descriptor.
    /// </summary>
    Remove,
}
