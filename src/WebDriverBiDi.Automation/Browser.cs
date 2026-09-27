// <copyright file="Browser.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;

/// <summary>
/// A user context of a <see cref="BrowserGroup"/>: pages with their own cookies, storage, and cache, isolated from
/// those of other browsers in the group.
/// </summary>
public sealed class Browser
{
    /// <summary>
    /// The ID of the user context every browser process has.
    /// </summary>
    public const string DefaultBrowserId = "default";

    private readonly object lockObject = new();
    private readonly List<Page> pages = [];
    private readonly ObservableEventInvocable<PageEventArgs> onPageCreated = new("automation.pageCreated");
    private readonly ObservableEventInvocable<PageEventArgs> onPageClosed = new("automation.pageClosed");

    /// <summary>
    /// Initializes a new instance of the <see cref="Browser"/> class.
    /// </summary>
    /// <param name="group">The group the browser belongs to.</param>
    /// <param name="id">The ID of the browser's user context.</param>
    internal Browser(BrowserGroup group, string id)
    {
        this.Group = group;
        this.Id = id;
    }

    /// <summary>
    /// Gets the ID of the browser's user context.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the group the browser belongs to.
    /// </summary>
    public BrowserGroup Group { get; }

    /// <summary>
    /// Gets a value indicating whether this is the browser of the default user context, which cannot be removed.
    /// </summary>
    public bool IsDefault => this.Id == DefaultBrowserId;

    /// <summary>
    /// Gets the browser's open pages, in the order they were found or created.
    /// </summary>
    public IReadOnlyList<Page> Pages
    {
        get
        {
            lock (this.lockObject)
            {
                return [.. this.pages];
            }
        }
    }

    /// <summary>
    /// Gets an observable event raised when a page opens in the browser, whether the library or the page opened it.
    /// </summary>
    public ObservableEvent<PageEventArgs> OnPageCreated => this.onPageCreated;

    /// <summary>
    /// Gets an observable event raised when a page of the browser closes.
    /// </summary>
    public ObservableEvent<PageEventArgs> OnPageClosed => this.onPageClosed;

    /// <summary>
    /// Opens a page in the browser.
    /// </summary>
    /// <param name="type">Whether the page opens as a tab or a window.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The page.</returns>
    public async Task<Page> NewPageAsync(CreateType type = CreateType.Tab, CancellationToken cancellationToken = default)
    {
        CreateCommandParameters parameters = new(type) { UserContextId = this.Id };
        CreateCommandResult result = await this.Group.Driver.BrowsingContext.CreateAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        return await this.AddPageAsync(result.BrowsingContextId, "about:blank").ConfigureAwait(false);
    }

    /// <summary>
    /// Closes the browser, removing its user context and closing its pages. The default browser cannot be removed,
    /// so closing it closes its pages and leaves it in the group.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>A task that completes when the browser is closed.</returns>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (this.IsDefault)
        {
            foreach (Page page in this.Pages)
            {
                await this.Group.Driver.BrowsingContext.CloseAsync(new WebDriverBiDi.BrowsingContext.CloseCommandParameters(page.Id), cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        await this.Group.Driver.Browser.RemoveUserContextAsync(new RemoveUserContextCommandParameters(this.Id), cancellationToken: cancellationToken).ConfigureAwait(false);
        this.Group.RemoveBrowser(this);
    }

    /// <summary>
    /// Adds a page, if the browser does not have it yet, raising <see cref="OnPageCreated"/> for a page it adds.
    /// </summary>
    /// <param name="id">The ID of the page's browsing context.</param>
    /// <param name="url">The URL of the page's main frame.</param>
    /// <returns>The page.</returns>
    internal async Task<Page> AddPageAsync(string id, string url)
    {
        Page page;
        lock (this.lockObject)
        {
            Page? existing = this.pages.Find(candidate => candidate.Id == id);
            if (existing is not null)
            {
                return existing;
            }

            page = new Page(this, id, url);
            this.pages.Add(page);
        }

        this.Group.RegisterFrame(page.MainFrame);
        await this.onPageCreated.InvokeNotifyObserversAsync(new PageEventArgs(page)).ConfigureAwait(false);
        return page;
    }

    /// <summary>
    /// Removes a page whose frames the group has detached, raising <see cref="OnPageClosed"/> and the page's own
    /// <see cref="Page.OnClosed"/>.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>A task that completes when observers are notified.</returns>
    internal async Task RemovePageAsync(Page page)
    {
        lock (this.lockObject)
        {
            this.pages.Remove(page);
        }

        await this.onPageClosed.InvokeNotifyObserversAsync(new PageEventArgs(page)).ConfigureAwait(false);
        await page.NotifyClosedAsync().ConfigureAwait(false);
    }
}
