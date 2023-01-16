// <copyright file="CommandSettings.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Abstract base class for a set of settings for a command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public abstract class CommandSettings
{
    private readonly Dictionary<string, object?> additionalData = new();

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public abstract string MethodName { get; }

    /// <summary>
    /// Gets the expected type of the result of the command.
    /// </summary>
    public abstract Type ResultType { get; }

    /// <summary>
    /// Gets additional properties to be serialized with this command.
    /// </summary>
    [JsonExtensionData]
    [JsonConverter(typeof(ResponseValueJsonConverter))]
    public Dictionary<string, object?> AdditionalData => this.additionalData;
}