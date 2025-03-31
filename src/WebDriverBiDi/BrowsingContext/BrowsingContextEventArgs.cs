// <copyright file="BrowsingContextEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

/// <summary>
/// Object containing event data for events raised by a browsing context being created or destroyed.
/// </summary>
public record BrowsingContextEventArgs : WebDriverBiDiEventArgs
{
    private readonly BrowsingContextInfo info;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowsingContextEventArgs"/> class.
    /// </summary>
    /// <param name="info">The BrowsingContextInfo used to create the event arguments.</param>
    public BrowsingContextEventArgs(BrowsingContextInfo info)
    {
        this.info = info;
    }

    /// <summary>
    /// Gets the browsing context ID of the browsing context.
    /// </summary>
    public string BrowsingContextId => this.info.BrowsingContextId;

    /// <summary>
    /// Gets the user context ID of the browsing context.
    /// </summary>
    public string UserContextId => this.info.UserContextId;

    /// <summary>
    /// Gets the browsing context ID of the original opener of this browsing context.
    /// </summary>
    public string? OriginalOpener => this.info.OriginalOpener;

    /// <summary>
    /// Gets the current URL of the browsing context.
    /// </summary>
    public string Url => this.info.Url;

    /// <summary>
    /// Gets the list of the child browsing contexts of the browsing context.
    /// </summary>
    public IList<BrowsingContextInfo> Children => this.info.Children;

    /// <summary>
    /// Gets the browsing context ID of the parent browsing context.
    /// </summary>
    public string? Parent => this.info.Parent;
}
