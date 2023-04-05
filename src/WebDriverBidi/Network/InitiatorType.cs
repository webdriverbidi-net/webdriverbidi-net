// <copyright file="InitiatorType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Enumerated values for network traffic initiators.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<InitiatorType>))]
public enum InitiatorType
{
    /// <summary>
    /// Network traffic initiated by the HTML parser.
    /// </summary>
    Parser,

    /// <summary>
    /// Network traffic initiated by the browser JavaScript engine.
    /// </summary>
    Script,

    /// <summary>
    /// Network traffic initiated by a CORS preflight request.
    /// </summary>
    Preflight,

    /// <summary>
    /// Network traffic initiated by the browser.
    /// </summary>
    Other,
}