// <copyright file="ProxyType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// The type of proxy.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ProxyType>))]
public enum ProxyType
{
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