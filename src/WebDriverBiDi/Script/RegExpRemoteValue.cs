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
public record RegExpRemoteValue : ValueHoldingRemoteValue<RegularExpressionValue>, IObjectReferenceRemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegExpRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal RegExpRemoteValue()
        : base(RemoteValueType.RegExp)
    {
    }

    /// <summary>
    /// Gets the regular expression value of this remote value.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonInclude]
    public override RegularExpressionValue Value { get; internal set; }

    /// <summary>
    /// Gets the handle of this RemoteValue.
    /// </summary>
    [JsonPropertyName("handle")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Handle { get; internal set; }

    /// <summary>
    /// Gets the internal ID of this RemoteValue.
    /// </summary>
    [JsonPropertyName("internalId")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InternalId { get; internal set; }

    /// <summary>
    /// Converts this RemoteValue into a RemoteObjectReference.
    /// </summary>
    /// <returns>The RemoteObjectReference object representing this RemoteValue.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when the RemoteValue does not have a handle set.</exception>
    public RemoteObjectReference ToRemoteObjectReference()
    {
        if (this.Handle is null)
        {
            throw new WebDriverBiDiException("RegExp remote values must have a valid handle to be used as remote references");
        }

        return new RemoteObjectReference(this.Handle);
    }

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
