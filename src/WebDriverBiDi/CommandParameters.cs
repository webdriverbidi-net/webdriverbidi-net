// <copyright file="CommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Text.Json.Serialization;
using WebDriverBiDi.BrowsingContext;

/// <summary>
/// Abstract base class for a set of settings for a command.
/// </summary>
public abstract class CommandParameters
{
    private readonly Dictionary<string, object?> additionalData = new();

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public abstract string MethodName { get; }

    /// <summary>
    /// Gets the type of the response for this command.
    /// </summary>
    [JsonIgnore]
    public abstract Type ResponseType { get; }

    /// <summary>
    /// Gets additional properties to be serialized with this command.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData => this.additionalData;
}
