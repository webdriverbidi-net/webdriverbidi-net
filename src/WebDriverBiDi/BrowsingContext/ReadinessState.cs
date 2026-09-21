// <copyright file="ReadinessState.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The stage of document loading at which a navigation command returns.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ReadinessState>))]
public enum ReadinessState
{
    /// <summary>
    /// Return once the navigation is committed.
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
