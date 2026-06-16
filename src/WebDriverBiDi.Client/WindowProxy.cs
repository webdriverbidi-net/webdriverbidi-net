// <copyright file="WindowProxy.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using WebDriverBiDi.Script;

/// <summary>
/// A proxy class for the global, window object of a browser.
/// </summary>
public class WindowProxy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowProxy"/> class.
    /// </summary>
    /// <param name="windowRef">The <see cref="RemoteObjectReference"/> representing the window.</param>
    internal WindowProxy(RemoteObjectReference windowRef)
    {
        this.WindowId = windowRef.Handle;
    }

    /// <summary>
    /// Gets the ID of the window this proxy object represents.
    /// </summary>
    public string WindowId { get; private set; }
}
