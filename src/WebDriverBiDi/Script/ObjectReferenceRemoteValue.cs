// <copyright file="ObjectReferenceRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a remote value for a null value from the remote end, providing
/// the ability to convert to a local value for use as an argument for script
/// execution on the remote end.
/// </summary>
public record ObjectReferenceRemoteValue : RemoteValue, IObjectReferenceRemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectReferenceRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal ObjectReferenceRemoteValue()
        : base()
    {
    }

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
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the null value.</returns>
    public override LocalValue ToLocalValue()
    {
        return this.ToRemoteReference();
    }

    /// <summary>
    /// Converts this RemoteValue into a RemoteReference.
    /// </summary>
    /// <returns>The RemoteReference object representing this RemoteValue.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the RemoteValue meets one of the following conditions:
    /// <list type="bulleted">
    ///   <item>
    ///     <description>
    ///       The RemoteValue is a primitive value (string, number, boolean, bigint, null, or undefined)
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The RemoteValue has a type of "node", but there is no shared ID set
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       The RemoteValue does not have a handle set
    ///     </description>
    ///   </item>
    /// </list>
    /// </exception>
    public virtual RemoteReference ToRemoteReference()
    {
        if (this.Handle is null)
        {
            throw new WebDriverBiDiException("Remote values must have a valid handle to be used as remote references");
        }

        return new RemoteObjectReference(this.Handle);
    }
}
