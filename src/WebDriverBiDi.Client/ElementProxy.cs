// <copyright file="ElementProxy.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using WebDriverBiDi.Script;

/// <summary>
/// A proxy class for an element in the DOM of a page.
/// </summary>
public class ElementProxy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementProxy"/> class.
    /// </summary>
    /// <param name="sharedRef">The <see cref="SharedReference"/> representing the element.</param>
    internal ElementProxy(SharedReference sharedRef)
    {
        this.ElementId = sharedRef.SharedId;
    }

    /// <summary>
    /// Gets the ID of the element this proxy represents.
    /// </summary>
    public string ElementId { get; private set; }
}
