// <copyright file="RemoteJavaScriptObjectProxy.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using WebDriverBiDi.Script;

/// <summary>
/// A proxy object for a JavaScript object in the browser.
/// </summary>
public class RemoteJavaScriptObjectProxy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteJavaScriptObjectProxy"/> class.
    /// </summary>
    /// <param name="objectRef">The <see cref="RemoteObjectReference"/> representing the remote object.</param>
    internal RemoteJavaScriptObjectProxy(RemoteObjectReference objectRef)
    {
        this.RemoteObjectId = objectRef.Handle;
    }

    /// <summary>
    /// Gets the ID of the remote JavaScript object.
    /// </summary>
    public string RemoteObjectId { get; private set; }
}
