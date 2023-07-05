// <copyright file="BytesValueType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

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