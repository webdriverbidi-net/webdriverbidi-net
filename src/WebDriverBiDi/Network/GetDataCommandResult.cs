// <copyright file="GetDataCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Result for retrieving data from a data collector for network traffic using the network.getData command.
/// </summary>
public class GetDataCommandResult : CommandResult
{
    private BytesValue bytes = BytesValue.FromString(string.Empty);

    [JsonConstructor]
    private GetDataCommandResult()
    {
    }

    /// <summary>
    /// Gets the bytes of the data collected by the network data collector.
    /// </summary>
    [JsonPropertyName("bytes")]
    [JsonRequired]
    [JsonInclude]
    public BytesValue Bytes { get => this.bytes; private set => this.bytes = value; }
}
