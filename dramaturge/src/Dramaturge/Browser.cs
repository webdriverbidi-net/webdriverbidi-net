// <copyright file="Browser.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using System.Text.RegularExpressions;
using Dramaturge.Network;
using WebDriverBiDi;
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
    private readonly List<RouteRegistration> routes = [];
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
    /// Adds a route for requests the browser's pages, their frames, and its workers make to a URL.
    /// </summary>
    /// <param name="url">The request's full URL.</param>
    /// <param name="handler">The handler, which answers, continues, or aborts each request.</param>
    /// <param name="filter">A pattern the browser matches first, so that only requests it matches are stopped, or <see langword="null"/> to stop every request while the route exists.</param>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    public Task<RouteRegistration> RouteAsync(string url, Func<Route, Task> handler, UrlPattern? filter = null, CancellationToken cancellationToken = default)
    {
        return this.AddRouteAsync(request => request.Url == url, url, handler, filter, cancellationToken);
    }

    /// <summary>
    /// Adds a route for requests the browser's pages, their frames, and its workers make to URLs matching a regular
    /// expression.
    /// </summary>
    /// <param name="url">The regular expression, matched against the request's full URL.</param>
    /// <param name="handler">The handler, which answers, continues, or aborts each request.</param>
    /// <param name="filter">A pattern the browser matches first, so that only requests it matches are stopped, or <see langword="null"/> to stop every request while the route exists.</param>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    public Task<RouteRegistration> RouteAsync(Regex url, Func<Route, Task> handler, UrlPattern? filter = null, CancellationToken cancellationToken = default)
    {
        return this.AddRouteAsync(request => url.IsMatch(request.Url), $"URLs matching {url}", handler, filter, cancellationToken);
    }

    /// <summary>
    /// Adds a route for requests the browser's pages, their frames, and its workers make that satisfy a condition,
    /// including a page's first request, such as a popup's. Requests are stopped before they are sent and handed to
    /// the routes of the page that made them, newest first, then to the browser's, newest first; a handler that
    /// does not answer, continue, or abort a request passes it to the next route that matches it, and a request no
    /// route decides, or whose handler throws, continues as it was, the exception reported on
    /// <see cref="BrowserGroup.OnLogMessage"/>. The protocol cannot limit stopping requests to one browser, so while
    /// the route exists, the requests of the group's other browsers that its filter matches are stopped too, and
    /// continued at once.
    /// </summary>
    /// <param name="request">The condition, given the request.</param>
    /// <param name="handler">The handler, which answers, continues, or aborts each request.</param>
    /// <param name="filter">A pattern the browser matches first, so that only requests it matches are stopped, or <see langword="null"/> to stop every request while the route exists. A pattern's parts are matched exactly; a part left out matches anything.</param>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    public Task<RouteRegistration> RouteAsync(Func<RequestData, bool> request, Func<Route, Task> handler, UrlPattern? filter = null, CancellationToken cancellationToken = default)
    {
        return this.AddRouteAsync(request, "requests satisfying the condition", handler, filter, cancellationToken);
    }

    /// <summary>
    /// Adds a route that answers requests the browser's pages, their frames, and its workers make from an HTTP Archive: a .har file, or a .zip holding one
    /// and its body files, as Playwright writes them. Each request is answered with the response of the entry of its
    /// method and URL, ignoring a fragment, whose recorded body is the request's, if both have one, comparing a
    /// multipart form's body without its boundary; of several, the one recorded with the most of the request's
    /// headers, then the first. A redirect is answered as recorded, and the browser follows it to the next entry. A
    /// request with no entry is aborted, unless <see cref="HarRouteOptions.NotFound"/> passes it on.
    /// </summary>
    /// <param name="harPath">The path of the archive, which is read now.</param>
    /// <param name="options">The route's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels reading the archive and the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    /// <exception cref="InvalidDataException">Thrown when the file is not an archive that can be read.</exception>
    public Task<RouteRegistration> RouteFromHarAsync(string harPath, HarRouteOptions? options = null, CancellationToken cancellationToken = default)
    {
        return this.AddHarRouteAsync(harPath, _ => true, "requests", options, cancellationToken);
    }

    /// <summary>
    /// Adds a route that answers requests the browser's pages, their frames, and its workers make to a URL from an HTTP Archive, as
    /// <see cref="RouteFromHarAsync(string, HarRouteOptions?, CancellationToken)"/> does.
    /// </summary>
    /// <param name="harPath">The path of the archive, which is read now.</param>
    /// <param name="url">The request's full URL.</param>
    /// <param name="options">The route's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels reading the archive and the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    /// <exception cref="InvalidDataException">Thrown when the file is not an archive that can be read.</exception>
    public Task<RouteRegistration> RouteFromHarAsync(string harPath, string url, HarRouteOptions? options = null, CancellationToken cancellationToken = default)
    {
        return this.AddHarRouteAsync(harPath, request => request.Url == url, url, options, cancellationToken);
    }

    /// <summary>
    /// Adds a route that answers requests the browser's pages, their frames, and its workers make to URLs matching a regular expression from an HTTP
    /// Archive, as <see cref="RouteFromHarAsync(string, HarRouteOptions?, CancellationToken)"/> does.
    /// </summary>
    /// <param name="harPath">The path of the archive, which is read now.</param>
    /// <param name="url">The regular expression, matched against the request's full URL.</param>
    /// <param name="options">The route's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels reading the archive and the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    /// <exception cref="InvalidDataException">Thrown when the file is not an archive that can be read.</exception>
    public Task<RouteRegistration> RouteFromHarAsync(string harPath, Regex url, HarRouteOptions? options = null, CancellationToken cancellationToken = default)
    {
        return this.AddHarRouteAsync(harPath, request => url.IsMatch(request.Url), $"URLs matching {url}", options, cancellationToken);
    }

    /// <summary>
    /// Adds a route that answers requests the browser's pages, their frames, and its workers make that satisfy a condition from an HTTP Archive, as
    /// <see cref="RouteFromHarAsync(string, HarRouteOptions?, CancellationToken)"/> does.
    /// </summary>
    /// <param name="harPath">The path of the archive, which is read now.</param>
    /// <param name="request">The condition, given the request.</param>
    /// <param name="options">The route's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels reading the archive and the commands.</param>
    /// <returns>The route, which can be removed.</returns>
    /// <exception cref="InvalidDataException">Thrown when the file is not an archive that can be read.</exception>
    public Task<RouteRegistration> RouteFromHarAsync(string harPath, Func<RequestData, bool> request, HarRouteOptions? options = null, CancellationToken cancellationToken = default)
    {
        return this.AddHarRouteAsync(harPath, request, "requests satisfying the condition", options, cancellationToken);
    }

    /// <summary>
    /// Starts recording the requests the browser's pages, their frames, and its workers make to an HTTP Archive file, with their responses and bodies. Disposing the
    /// recording writes the file; <see cref="HarRecording.SaveAsync"/> writes it sooner. The file can be replayed with
    /// <see cref="RouteFromHarAsync(string, HarRouteOptions?, CancellationToken)"/>.
    /// </summary>
    /// <param name="harPath">The path of the .har file to write.</param>
    /// <param name="options">The recording's settings, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels starting.</param>
    /// <returns>The recording.</returns>
    public Task<HarRecording> RecordHarAsync(string harPath, HarRecordingOptions? options = null, CancellationToken cancellationToken = default)
    {
        return HarRecording.StartAsync(this.Group, harPath, options, monitorOptions => monitorOptions.UserContextIds.Add(this.Id), cancellationToken);
    }

    /// <summary>
    /// Removes every route of the browser, but not those of its pages. A request already stopped is still handled.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>A task that completes when the routes are removed.</returns>
    public async Task UnrouteAllAsync(CancellationToken cancellationToken = default)
    {
        List<RouteRegistration> removed;
        lock (this.lockObject)
        {
            removed = [.. this.routes];
        }

        foreach (RouteRegistration route in removed)
        {
            await route.RemoveAsync(cancellationToken).ConfigureAwait(false);
        }
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

        // A browser's routes stop the requests of every browser, so they must not outlive it.
        await this.UnrouteAllAsync(cancellationToken).ConfigureAwait(false);
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

    /// <summary>
    /// Hands a stopped request to the routes that stopped it: the page's, newest first, then the browser's, newest
    /// first, continuing it if none decides.
    /// </summary>
    /// <param name="page">The page that made the request, or <see langword="null"/> if no tracked page did.</param>
    /// <param name="frame">The frame that made the request, or <see langword="null"/> if no tracked frame did.</param>
    /// <param name="e">The request's event.</param>
    /// <returns>A task that completes when the request has been handled.</returns>
    internal async Task HandleBlockedRequestAsync(Page? page, Frame? frame, BeforeRequestSentEventArgs e)
    {
        List<RouteRegistration> stopping = page?.FindStoppingRoutes(e.Intercepts!) ?? [];
        lock (this.lockObject)
        {
            stopping.AddRange(this.routes.FindAll(route => e.Intercepts!.Contains(route.InterceptId)));
        }

        Route routed = new(this, page, frame, e.Request);
        foreach (RouteRegistration route in stopping)
        {
            try
            {
                if (!route.Matches(e.Request))
                {
                    continue;
                }

                await route.Handler(routed).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this.Group.LogAsync($"The route for {route.Description} failed handling a request to {e.Request.Url}: {ex.Message}", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
                break;
            }

            if (routed.IsHandled)
            {
                return;
            }
        }

        if (!routed.IsHandled)
        {
            await this.Group.ContinueRequestAsync(e.Request).ConfigureAwait(false);
        }
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

    private void RemoveRoute(RouteRegistration route)
    {
        lock (this.lockObject)
        {
            this.routes.Remove(route);
        }
    }

    private async Task<RouteRegistration> AddHarRouteAsync(string harPath, Func<RequestData, bool> matches, string description, HarRouteOptions? options, CancellationToken cancellationToken)
    {
        HarRouter router = await HarRouter.CreateAsync(this.Group, harPath, options, parameters => parameters.UserContexts.Add(this.Id), cancellationToken).ConfigureAwait(false);
        try
        {
            return await this.AddRouteAsync(matches, $"{description} from the HAR {Path.GetFileName(harPath)}", router.HandleAsync, options?.Filter, cancellationToken, router.RemoveAsync).ConfigureAwait(false);
        }
        catch
        {
            await router.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }
    }

    // Each route has its own intercept, with its filter, for every page of every browser: an intercept can be limited
    // to pages, but not to a user context.
    private async Task<RouteRegistration> AddRouteAsync(Func<RequestData, bool> matches, string description, Func<Route, Task> handler, UrlPattern? filter, CancellationToken cancellationToken, Func<CancellationToken, Task>? removing = null)
    {
        await this.Group.EnsureNetworkEventsAsync(cancellationToken).ConfigureAwait(false);
        AddInterceptCommandParameters parameters = new(InterceptPhase.BeforeRequestSent);
        if (filter is not null)
        {
            parameters.UrlPatterns.Add(filter);
        }

        string interceptId = (await this.Group.Driver.Network.AddInterceptAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false)).InterceptId;
        this.Group.TrackIntercept(interceptId);
        RouteRegistration route = new(this.Group, this.RemoveRoute, matches, description, handler, interceptId, removing);
        lock (this.lockObject)
        {
            this.routes.Insert(0, route);
        }

        return route;
    }
}
