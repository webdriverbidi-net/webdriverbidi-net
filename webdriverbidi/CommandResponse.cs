// <copyright file="CommandResponse.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using Newtonsoft.Json;

/// <summary>
/// Base class for the result of a command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public abstract class CommandResponse : WebDriverBidiMessage
{
    private long id;

    /// <summary>
    /// Gets the ID for this command during execution.
    /// </summary>
    [JsonProperty("id")]
    [JsonRequired]
    public long Id { get => this.id; private set => this.id = value; }

    /// <summary>
    /// Gets the result data for the command.
    /// </summary>
    public abstract ResponseData Result { get; }
}