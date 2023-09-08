// <copyright file="ReadinessState.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The readiness state of the browsing context.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ReadinessState>))]
public enum ReadinessState
{
    /// <summary>
    /// Return immediately without checking for readiness state.
    /// </summary>
    None,

    /// <summary>
    /// Return after the readiness state becomes "interactive".
    /// </summary>
    Interactive,

    /// <summary>
    /// Return after the readiness state becomes "complete".
    /// </summary>
    Complete,
}