// <copyright file="EventSubscriptionHandle.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

/// <summary>
/// The handle an event subscription call returns.
/// </summary>
internal sealed class EventSubscriptionHandle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventSubscriptionHandle"/> class.
    /// </summary>
    /// <param name="handleTypeName">The name of the handle's type.</param>
    /// <param name="methodName">The name of the method that returned the handle.</param>
    public EventSubscriptionHandle(string handleTypeName, string methodName)
    {
        this.HandleTypeName = handleTypeName;
        this.MethodName = methodName;
    }

    /// <summary>
    /// Gets the name of the handle's type.
    /// </summary>
    public string HandleTypeName { get; }

    /// <summary>
    /// Gets the name of the method that returned the handle.
    /// </summary>
    public string MethodName { get; }
}
