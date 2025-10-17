// <copyright file="SimulateServiceType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the simulation of services.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<SimulateServiceType>))]
public enum SimulateServiceType
{
    /// <summary>
    /// Simulate adding a service.
    /// </summary>
    Add,

    /// <summary>
    /// Simulate removing a service.
    /// </summary>
    Remove,
}
