// <copyright file="ProxyType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The type of proxy.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ProxyType>))]
public enum ProxyType
{
    /// <summary>
    /// No proxy value has been set.
    /// TODO: Remove this enum value once https://bugzilla.mozilla.org/show_bug.cgi?id=1916463 is fixed.
    /// </summary>
    Unset,

    /// <summary>
    /// Direct connection with no proxy.
    /// </summary>
    Direct,

    /// <summary>
    /// Use the proxy registered in the system.
    /// </summary>
    System,

    /// <summary>
    /// Use a manually configured proxy.
    /// </summary>
    Manual,

    /// <summary>
    /// Automatically detect the type of proxy to use.
    /// </summary>
    AutoDetect,

    /// <summary>
    /// Use a proxy autoconfig (PAC) file.
    /// </summary>
    [JsonEnumValue("pac")]
    ProxyAutoConfig,
}
