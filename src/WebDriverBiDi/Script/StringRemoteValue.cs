// <copyright file="StringRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a string from the remote end, providing type-safe
/// access to the string value and the ability to convert to a local value for use
/// as an argument for script execution on the remote end.
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public record StringRemoteValue : ValueHoldingRemoteValue<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringRemoteValue"/> class.
    /// </summary>
    /// <param name="value">The string value.</param>
    internal StringRemoteValue(string value)
        : base(RemoteValueType.String)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets or setsthe string value of this remote value.
    /// </summary>
    public override string Value { get; protected set; }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the string value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.String(this.Value);
}
