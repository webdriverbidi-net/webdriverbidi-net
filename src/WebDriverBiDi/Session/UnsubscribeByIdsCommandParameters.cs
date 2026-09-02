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
    /// Initializes a new instance of the <see cref="UnsubscribeByIdsCommandParameters"/> class for a single subscription.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription from which to unsubscribe.</param>
    public UnsubscribeByIdsCommandParameters(string subscriptionId)
        : this([subscriptionId])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubscribeByIdsCommandParameters"/> class.
    /// </summary>
    /// <remarks>
    /// The specification requires an unsubscription to name at least one subscription ID.
    /// Unlike values whose specification constraints the remote end enforces, an empty
    /// subscription ID list is rejected here: an unsubscription from no subscriptions cannot
    /// be meaningful under any revision of the specification, and accepting it would only
    /// defer a certain failure to the remote end.
    /// </remarks>
    /// <param name="subscriptionIds">The list of IDs of subscriptions from which to unsubscribe.</param>
    /// <exception cref="ArgumentException">Thrown when no subscription IDs are specified in the subscription ID list.</exception>
    public UnsubscribeByIdsCommandParameters(IList<string> subscriptionIds)
        : base()
    {
        if (subscriptionIds.Count == 0)
        {
            throw new ArgumentException("At least one subscription ID must be specified.", nameof(subscriptionIds));
        }

        this.SubscriptionIds.AddRange(subscriptionIds);
    }

    /// <summary>
    /// Gets the list of subscription IDs from which to unsubscribe.
    /// </summary>
    [JsonPropertyName("subscriptions")]
    public List<string> SubscriptionIds { get; } = [];
}
