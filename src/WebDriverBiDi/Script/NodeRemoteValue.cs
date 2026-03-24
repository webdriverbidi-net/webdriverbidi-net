// <copyright file="NodeRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Represents a remote value for a DOM node, providing type-safe access to the
/// NodeProperties value and the ability to convert to a local value for use as
/// an argument for script execution on the remote end..
/// </summary>
[JsonConverter(typeof(RemoteValueJsonConverter))]
public record NodeRemoteValue : ValueHoldingRemoteValue<NodeProperties>, IObjectReferenceRemoteValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NodeRemoteValue"/> class.
    /// </summary>
    /// <param name="value">The NodeProperties value for the remote node.</param>
    [JsonConstructor]
    internal NodeRemoteValue(NodeProperties value)
        : base(RemoteValueType.Node)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets or setsthe NodeProperties value of this remote value.
    /// </summary>
    public override NodeProperties Value { get; protected set; }

    /// <summary>
    /// Gets the handle of this RemoteValue.
    /// </summary>
    public string? Handle { get; internal set; }

    /// <summary>
    /// Gets the internal ID of this RemoteValue.
    /// </summary>
    public string? InternalId { get; internal set; }

    /// <summary>
    /// Gets the shared ID of this RemoteValue.
    /// </summary>
    public string? SharedId { get; internal set; }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the DOM node.</returns>
    public override LocalValue ToLocalValue()
    {
        return this.ToSharedReference();
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
    public RemoteReference ToRemoteReference()
    {
        return this.ToSharedReference();
    }

    /// <summary>
    /// Converts this RemoteValue to a SharedReference.
    /// </summary>
    /// <returns>The SharedReference object representing this RemoteValue.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if this RemoteValue cannot be converted to a SharedReference.</exception>
    public SharedReference ToSharedReference()
    {
        if (this.SharedId is null)
        {
            throw new WebDriverBiDiException("Node remote values must have a valid shared ID to be used as remote references");
        }

        return new SharedReference(this.SharedId) { Handle = this.Handle };
    }
}
