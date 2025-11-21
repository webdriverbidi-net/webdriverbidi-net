// <copyright file="NetworkConditionsOffline.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// A class for emulating the network conditions of a device being offline.
/// </summary>
public class NetworkConditionsOffline : NetworkConditions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkConditionsOffline"/> class.
    /// </summary>
    public NetworkConditionsOffline()
        : base()
    {
    }

    /// <summary>
    /// Gets the type of clip rectangle.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonInclude]
    public override string Type => "offline";
}
