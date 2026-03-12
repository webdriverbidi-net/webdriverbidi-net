// <copyright file="CommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Text.Json.Serialization;

/// <summary>
/// Abstract base class for a set of settings for a command.
/// </summary>
public abstract class CommandParameters
{
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
    /// <remarks>
    /// <para>
    /// The WebDriver BiDi protocol allows for additional command properties that are not
    /// defined in the specification. This dictionary allows users to include those additional
    /// properties while maintaining the strongly-typed nature of the CommandParameters class.
    /// The entries in this dictionary are intentionally mutable, so that users can add additional
    /// properties as needed.
    /// </para>
    /// <para>
    /// Though the values of this dictionary are typed as <see cref="object"/>, they will be
    /// serialized to JSON using the same rules as other properties of the command. This means
    /// that values can be of any type that is serializable to JSON, including complex objects,
    /// arrays, and primitive types. However, custom types should be designed with JSON
    /// serialization in mind to ensure they are serialized correctly, including use of
    /// the appropriate attributes for JSON serialization by
    /// <see cref="System.Text.Json.JsonSerializer"/>.
    /// </para>
    /// </remarks>
    [JsonIgnore]
    public Dictionary<string, object?> AdditionalData { get; } = [];
}
