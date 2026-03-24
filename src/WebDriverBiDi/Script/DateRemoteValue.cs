// <copyright file="DateRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a date object from the remote end, providing
/// type-safe access to the value and the ability to convert to a local value
/// for use as an argument for script execution on the remote end.
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public record DateRemoteValue : ValueHoldingRemoteValue<DateTime>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateRemoteValue"/> class.
    /// </summary>
    /// <param name="value">The DateTime value.</param>
    internal DateRemoteValue(DateTime value)
        : base(RemoteValueType.Date)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets or sets the DateTime value of this remote value.
    /// </summary>
    public override DateTime Value { get; protected set; }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the date value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.Date(this.Value);
}
