// <copyright file="UnsubscribeByIdsCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the session.unsubscribe command.
/// </summary>
public class UnsubscribeByIdsCommandParameters : UnsubscribeCommandParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubscribeByIdsCommandParameters"/> class.
    /// </summary>
    public UnsubscribeByIdsCommandParameters()
        : base()
    {
    }

    /// <summary>
    /// Gets the list of events to which to subscribe or unsubscribe.
    /// </summary>
    [JsonPropertyName("subscriptions")]
    public List<string> SubscriptionIds { get; } = [];
}
