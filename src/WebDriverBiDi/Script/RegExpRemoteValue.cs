// <copyright file="RegExpRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a regular expression object from the remote end,
/// providing type-safe access to theRegularExpressionValue value and the ability
/// to convert to a local value for use as an argument for script execution on the
/// remote end.
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public record RegExpRemoteValue : ValueHoldingRemoteValue<RegularExpressionValue>, IObjectReferenceRemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegExpRemoteValue"/> class.
    /// </summary>
    /// <param name="value">The regular expression value.</param>
    internal RegExpRemoteValue(RegularExpressionValue value)
        : base(RemoteValueType.RegExp)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets or setsthe regular expression value of this remote value.
    /// </summary>
    public override RegularExpressionValue Value { get; protected set; }

    /// <summary>
    /// Gets the handle of this RemoteValue.
    /// </summary>
    public string? Handle { get; internal set; }

    /// <summary>
    /// Gets the internal ID of this RemoteValue.
    /// </summary>
    public string? InternalId { get; internal set; }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the regular expression value.</returns>
    public override LocalValue ToLocalValue()
    {
        RegularExpressionValue regexValue = this.Value;
        return LocalValue.RegExp(regexValue.Pattern, regexValue.Flags);
    }
}
