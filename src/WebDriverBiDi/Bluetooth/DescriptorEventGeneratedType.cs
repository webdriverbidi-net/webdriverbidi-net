// <copyright file="DescriptorEventGeneratedType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Bluetooth;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the type of characteristic event generated.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<DescriptorEventGeneratedType>))]
public enum DescriptorEventGeneratedType
{
    /// <summary>
    /// The descriptor event is a read event.
    /// </summary>
    Read,

    /// <summary>
    /// The descriptor event is a write event.
    /// </summary>
    Write,
}
