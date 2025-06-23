// <copyright file="CollectorType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated value of types for a Collector.
/// Note: This exists as an enum in expectation of future other collector types like "stream".
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<CollectorType>))]
public enum CollectorType
{
    /// <summary>
    /// The CollectorType collects data as a blob.
    /// </summary>
    Blob,
}
