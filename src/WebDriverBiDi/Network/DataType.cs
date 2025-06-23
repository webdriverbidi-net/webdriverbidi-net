// <copyright file="DataType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated value of data being collected by a network data collector.
/// Note: This exists as an enum in expectation of future other data types like "request".
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<DataType>))]
public enum DataType
{
    /// <summary>
    /// The type of data being collected is from network responses.
    /// </summary>
    Response,
}
