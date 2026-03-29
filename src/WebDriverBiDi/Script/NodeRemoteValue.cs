// <copyright file="NodeRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a remote value for a DOM node, providing type-safe access to the
/// NodeProperties value and the ability to convert to a local value for use as
/// an argument for script execution on the remote end..
/// </summary>
public record NodeRemoteValue : RemoteValue, IObjectReferenceRemoteValue, ITypeSafeRemoteValue<NodeProperties?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NodeRemoteValue"/> class.
    /// </summary>
    [JsonConstructor]
    internal NodeRemoteValue()
    {
        this.Type = RemoteValueType.Node;
    }

    /// <summary>
    /// Gets the NodeProperties value of this remote value.
    /// </summary>
    /// <remarks>
    /// This value may be null if the remote value was deserialized without a value property.
    /// </remarks>
    [JsonPropertyName("value")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NodeProperties? Value { get; internal set; }

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
    /// Gets the shared ID of this RemoteValue.
    /// </summary>
    [JsonPropertyName("sharedId")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SharedId { get; internal set; }

    /// <summary>
    /// Gets the properties of the node represented by the Value property.
    /// </summary>
    /// <returns>The NodeProperties object representing the node.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the node properties are not returned from the remote end.</exception>
    /// <remarks>
    /// The Value property may be null if the remote value was deserialized without a value
    /// property. This method provides a way to avoid having to check for null when using
    /// the Value property.
    /// </remarks>
    public NodeProperties GetNodeProperties()
    {
        if (this.Value is null)
        {
            throw new WebDriverBiDiException("Node remote value does not have a value property set.");
        }

        return this.Value;
    }

    /// <summary>
    /// Converts this remote value to a local value for use as an argument for script execution on the remote end.
    /// </summary>
    /// <returns>A LocalValue representing the DOM node.</returns>
    public override LocalValue ToLocalValue()
    {
        return this.ToSharedReference();
    }

    /// <summary>
    /// Converts this RemoteValue into a RemoteObjectReference.
    /// </summary>
    /// <returns>The RemoteObjectReference object representing this RemoteValue.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when there is no shared ID set.</exception>
    public RemoteObjectReference ToRemoteObjectReference()
    {
        if (this.Handle is null)
        {
            throw new WebDriverBiDiException("Node remote values must have a valid handle to be used as remote references");
        }

        return new(this.Handle) { SharedId = this.SharedId };
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
