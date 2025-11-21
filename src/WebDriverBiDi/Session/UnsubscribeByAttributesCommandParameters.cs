// <copyright file="UnsubscribeByAttributesCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Session;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the session.unsubscribe command.
/// </summary>
public class UnsubscribeByAttributesCommandParameters : UnsubscribeCommandParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubscribeByAttributesCommandParameters"/> class.
    /// </summary>
    public UnsubscribeByAttributesCommandParameters()
        : base()
    {
    }

    /// <summary>
    /// Gets the list of events to which to subscribe or unsubscribe.
    /// </summary>
    [JsonPropertyName("events")]
    public List<string> Events { get; } = [];
}
