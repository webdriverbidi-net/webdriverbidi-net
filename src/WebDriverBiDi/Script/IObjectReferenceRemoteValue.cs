// <copyright file="IObjectReferenceRemoteValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

/// <summary>
/// Interface for a remote value that represents an object reference, providing access to the
/// handle and internal ID of the remote value, which can be used to reference the object on the
/// remote end.
/// </summary>
public interface IObjectReferenceRemoteValue
{
    /// <summary>
    /// Gets the handle of this RemoteValue.
    /// </summary>
    string? Handle { get; }

    /// <summary>
    /// Gets the internal ID of this RemoteValue.
    /// </summary>
    string? InternalId { get; }

    /// <summary>
    /// Converts this RemoteValue into a RemoteObjectReference.
    /// </summary>
    /// <returns>The RemoteObjectReference object representing this RemoteValue.</returns>
    RemoteObjectReference ToRemoteObjectReference();
}
