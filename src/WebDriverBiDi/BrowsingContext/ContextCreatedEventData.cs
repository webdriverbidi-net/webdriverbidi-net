// <copyright file="ContextCreatedEventData.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// The data received with the browsingContext.contextCreated event, exposed to observers
/// through <see cref="ContextCreatedEventArgs"/>.
/// </summary>
public record ContextCreatedEventData : BrowsingContextInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContextCreatedEventData"/> class.
    /// </summary>
    [JsonConstructor]
    internal ContextCreatedEventData()
        : base()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the browsing context will navigate after the event is raised.
    /// </summary>
    [JsonPropertyName("hasPlannedNavigation")]
    [JsonRequired]
    [JsonInclude]
    public bool HasPlannedNavigation { get; internal set; }
}
