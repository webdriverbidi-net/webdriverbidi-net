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
public record DateRemoteValue : ValueHoldingRemoteValue<DateTime>, IObjectReferenceRemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal DateRemoteValue()
        : base(RemoteValueType.Date)
    {
    }

    /// <summary>
    /// Gets the DateTime value of this remote value.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonInclude]
    [JsonRequired]
    public override DateTime Value { get; internal set; } = DateTime.MinValue;

    /// <summary>
    /// Gets the handle of this remote value.
    /// </summary>
    [JsonPropertyName("handle")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Handle { get; internal set; }

    /// <summary>
    /// Gets the internal ID of this remote value.
    /// </summary>
    [JsonPropertyName("internalId")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InternalId { get; internal set; }

    /// <summary>
    /// Defines an implicit conversion from a DateRemoteValue to a DateTime, allowing
    /// for easy access to the date value of this remote value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator DateTime(DateRemoteValue value) => value.Value;

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the date value.</returns>
    public override LocalValue ToLocalValue() => LocalValue.Date(this.Value);

    /// <summary>
    /// Converts this RemoteValue into a RemoteObjectReference.
    /// </summary>
    /// <returns>The RemoteObjectReference object representing this RemoteValue.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when there is no shared ID set.</exception>
    public RemoteObjectReference ToRemoteObjectReference()
    {
        if (this.Handle is null)
        {
            throw new WebDriverBiDiException("Date remote values must have a valid handle to be used as remote references");
        }

        return new RemoteObjectReference(this.Handle);
    }
}
