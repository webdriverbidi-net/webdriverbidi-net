// <copyright file="ContextDestroyedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

/// <summary>
/// Object containing event data for the browsingContext.contextDestroyed event.
/// </summary>
public record ContextDestroyedEventArgs : WebDriverBiDiEventArgs
{
    private readonly BrowsingContextInfo info;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextDestroyedEventArgs"/> class.
    /// </summary>
    /// <param name="info">The BrowsingContextInfo describing the destroyed browsing context.</param>
    public ContextDestroyedEventArgs(BrowsingContextInfo info)
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
    /// Gets the ID of the client window of this browsing context.
    /// </summary>
    public string ClientWindowId => this.info.ClientWindowId;

    /// <summary>
    /// Gets the browsing context ID of the original opener of this browsing context.
    /// </summary>
    public string? OriginalOpener => this.info.OriginalOpener;

    /// <summary>
    /// Gets the URL of the browsing context.
    /// </summary>
    public string Url => this.info.Url;

    /// <summary>
    /// Gets the list of the child browsing contexts of the browsing context, or
    /// <see langword="null"/> if the child contexts were not enumerated. An empty list means the
    /// context was enumerated and has no children.
    /// </summary>
    public IList<BrowsingContextInfo>? Children => this.info.Children;

    /// <summary>
    /// Gets the browsing context ID of the parent browsing context.
    /// </summary>
    public string? Parent => this.info.Parent;
}
