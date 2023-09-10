// <copyright file="InterceptPhase.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for setting up intercepts for network traffic.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<InterceptPhase>))]
public enum InterceptPhase
{
    /// <summary>
    /// Network traffic will be intercepted before a request is sent.
    /// </summary>
    [JsonEnumValue("beforeRequestSent")]
    BeforeRequestSent,

    /// <summary>
    /// Network traffic will be intercepted when a response is received, but before sent to the browser.
    /// </summary>
    [JsonEnumValue("responseStarted")]
    ResponseStarted,

    /// <summary>
    /// Network traffic will be intercepted when a response would require authorization.
    /// </summary>
    [JsonEnumValue("authRequired")]
    AuthRequired,
}