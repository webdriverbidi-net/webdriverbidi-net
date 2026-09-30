// <copyright file="Browser.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Network;
using WebDriverBiDi.Storage;

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
    /// Allows the browser's pages to download files, saving them in a folder.
    /// </summary>
    /// <param name="destinationFolder">The folder, on the machine the browser runs on.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the behavior is set.</returns>
    public Task AllowDownloadsAsync(string destinationFolder, CancellationToken cancellationToken = default)
    {
        return this.SetDownloadBehaviorAsync(new DownloadBehaviorAllowed(destinationFolder), cancellationToken);
    }

    /// <summary>
    /// Stops the browser's pages from downloading files; a download they begin is canceled.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the behavior is set.</returns>
    public Task DenyDownloadsAsync(CancellationToken cancellationToken = default)
    {
        return this.SetDownloadBehaviorAsync(new DownloadBehaviorDenied(), cancellationToken);
    }

    /// <summary>
    /// Returns the browser's downloads to the behavior the session started with.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the behavior is reset.</returns>
    public Task ResetDownloadBehaviorAsync(CancellationToken cancellationToken = default)
    {
        return this.SetDownloadBehaviorAsync(null, cancellationToken);
    }

    /// <summary>
    /// Gets the browser's cookies, optionally only those of a domain or with a name.
    /// </summary>
    /// <param name="domain">The domain, or <see langword="null"/> for any.</param>
    /// <param name="name">The name, or <see langword="null"/> for any.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The cookies.</returns>
    public async Task<IReadOnlyList<BrowserCookie>> GetCookiesAsync(string? domain = null, string? name = null, CancellationToken cancellationToken = default)
    {
        GetCookiesCommandParameters parameters = new() { Filter = CreateFilter(domain, name), Partition = this.CreatePartition() };
        GetCookiesCommandResult result = await this.Group.Driver.Storage.GetCookiesAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        return [.. result.Cookies.Select(cookie => new BrowserCookie(cookie.Name, ReadValue(cookie.Value), cookie.Domain)
        {
            Path = cookie.Path,
            HttpOnly = cookie.HttpOnly,
            Secure = cookie.Secure,
            SameSite = cookie.SameSite,
            Expires = cookie.Expires,
        })];
    }

    /// <summary>
    /// Adds cookies to the browser, replacing any with the same name, domain, and path.
    /// </summary>
    /// <param name="cookies">The cookies.</param>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>A task that completes when the cookies are added.</returns>
    public async Task AddCookiesAsync(IEnumerable<BrowserCookie> cookies, CancellationToken cancellationToken = default)
    {
        foreach (BrowserCookie cookie in cookies)
        {
            PartialCookie partial = new(cookie.Name, BytesValue.FromString(cookie.Value), cookie.Domain)
            {
                Path = cookie.Path,
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure,
                SameSite = cookie.SameSite,
                Expires = cookie.Expires,
            };
            await this.Group.Driver.Storage.SetCookieAsync(new SetCookieCommandParameters(partial) { Partition = this.CreatePartition() }, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Deletes the browser's cookies, optionally only those of a domain or with a name.
    /// </summary>
    /// <param name="domain">The domain, or <see langword="null"/> for any.</param>
    /// <param name="name">The name, or <see langword="null"/> for any.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the cookies are deleted.</returns>
    public Task ClearCookiesAsync(string? domain = null, string? name = null, CancellationToken cancellationToken = default)
    {
        DeleteCookiesCommandParameters parameters = new() { Filter = CreateFilter(domain, name), Partition = this.CreatePartition() };
        return this.Group.Driver.Storage.DeleteCookiesAsync(parameters, cancellationToken: cancellationToken);
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
    /// Adds a page, if the browser does not have it yet, raising <see cref="OnPageCreated"/> for a page it adds,
    /// then its opener's <see cref="Page.OnPopup"/>.
    /// </summary>
    /// <param name="id">The ID of the page's browsing context.</param>
    /// <param name="url">The URL of the page's main frame.</param>
    /// <param name="opener">The page that opened this one, or <see langword="null"/> if none did or it is not tracked.</param>
    /// <returns>The page.</returns>
    internal async Task<Page> AddPageAsync(string id, string url, Page? opener = null)
    {
        Page page;
        lock (this.lockObject)
        {
            Page? existing = this.pages.Find(candidate => candidate.Id == id);
            if (existing is not null)
            {
                return existing;
            }

            page = new Page(this, id, url, opener);
            this.pages.Add(page);
        }

        this.Group.RegisterFrame(page.MainFrame);
        await this.onPageCreated.InvokeNotifyObserversAsync(new PageEventArgs(page)).ConfigureAwait(false);
        if (opener is not null)
        {
            await opener.NotifyPopupAsync(page).ConfigureAwait(false);
        }

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

    private static CookieFilter? CreateFilter(string? domain, string? name)
    {
        return domain is null && name is null ? null : new CookieFilter() { Domain = domain, Name = name };
    }

    private static string ReadValue(BytesValue value)
    {
        return value.Type == BytesValueType.String ? value.Value : System.Text.Encoding.UTF8.GetString(value.ValueAsByteArray);
    }

    // Cookies are kept per user context, which is the browser.
    private StorageKeyPartitionDescriptor CreatePartition()
    {
        return new StorageKeyPartitionDescriptor() { UserContextId = this.Id };
    }

    private Task SetDownloadBehaviorAsync(DownloadBehavior? behavior, CancellationToken cancellationToken)
    {
        SetDownloadBehaviorCommandParameters parameters = new() { DownloadBehavior = behavior };
        parameters.UserContexts.Add(this.Id);
        return this.Group.Driver.Browser.SetDownloadBehaviorAsync(parameters, cancellationToken: cancellationToken);
    }
}
