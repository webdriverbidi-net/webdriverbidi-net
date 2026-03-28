// <copyright file="StringRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a remote value for a string from the remote end, providing type-safe
/// access to the string value and the ability to convert to a local value for use
/// as an argument for script execution on the remote end.
/// </summary>
public record StringRemoteValue : ValueHoldingRemoteValue<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal StringRemoteValue()
        : base(RemoteValueType.String)
    {
    }

    /// <summary>
    /// Gets the string value of this remote value.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonInclude]
    [JsonRequired]
    public override string Value { get; internal set; } = string.Empty;

    /// <summary>
    /// Defines an implicit conversion from a StringRemoteValue to a string, allowing
    /// for easy access to the string value of this remote value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator string(StringRemoteValue value) => value.Value;

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the string value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.String(this.Value);
}
