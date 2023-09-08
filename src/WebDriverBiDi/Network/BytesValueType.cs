// <copyright file="BytesValueType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated value of types for a BytesValue.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<BytesValueType>))]
public enum BytesValueType
{
    /// <summary>
    /// The BytesValue represents a string.
    /// </summary>
    String,

    /// <summary>
    /// The BytesValue represents a byte array as a base64-encoded string.
    /// </summary>
    Base64,
}