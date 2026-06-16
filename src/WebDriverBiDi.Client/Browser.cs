// <copyright file="Browser.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Elements;

/// <summary>
/// Provides a high-level abstraction over a WebDriver BiDi user context, tracking the top-level
/// <see cref="Page"/> objects that belong to it.
/// </summary>
public class Browser : IAsyncDisposable
{
    private readonly BiDiDriver driver;
    private readonly BrowserGroup repository;
    private readonly ElementLocatorSettings locatorSettings;
    private readonly List<Page> pages = [];
    private readonly EventObserver<BrowsingContextEventArgs> contextCreatedObserver;
    private readonly EventObserver<BrowsingContextEventArgs> contextDestroyedObserver;
    private readonly ElementStateInspector inspector;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Browser"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance used for executing commands.</param>
    /// <param name="userContextId">The ID of the user context this browser represents.</param>
    /// <param name="repository">The <see cref="BrowserGroup"/> that owns this browser.</param>
    /// <param name="locatorSettings">The <see cref="ElementLocatorSettings"/> to apply to element locators.</param>
    internal Browser(BiDiDriver driver, string userContextId, BrowserGroup repository, ElementLocatorSettings locatorSettings)
    {
        this.driver = driver;
        this.Id = userContextId;
        this.repository = repository;
        this.locatorSettings = locatorSettings;

        this.contextCreatedObserver = this.driver.BrowsingContext.OnContextCreated.AddObserver(this.OnContextCreated);
        this.contextDestroyedObserver = this.driver.BrowsingContext.OnContextDestroyed.AddObserver(this.OnContextDestroyed);
        this.inspector = repository.ElementStateInspector;
    }

    /// <summary>
    /// Gets the ID of this browser.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets a read-only list of <see cref="Page"/> objects representing the top-level browsing
    /// contexts currently open in this user context.
    /// </summary>
    public IReadOnlyList<Page> Pages => this.pages.AsReadOnly();

    /// <summary>
    /// Asynchronously releases all resources associated with this user context, including removing
    /// it from the owning <see cref="BrowserGroup"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        this.contextCreatedObserver.Dispose();
        this.contextDestroyedObserver.Dispose();

        try
        {
            await this.driver.Browser.RemoveUserContextAsync(
                new WebDriverBiDi.Browser.RemoveUserContextCommandParameters(this.Id))
                .ConfigureAwait(false);
        }
        catch
        {
            // Suppress: removal is best-effort during disposal.
        }

        this.repository.RemoveBrowser(this);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Adds a <see cref="Page"/> to this browser's page list. Used during initial population
    /// from the browsing context tree.
    /// </summary>
    /// <param name="page">The page to add.</param>
    internal void AddPage(Page page)
    {
        this.pages.Add(page);
    }

    private void OnContextCreated(BrowsingContextEventArgs args)
    {
        if (args.Parent is not null)
        {
            return;
        }

        if (args.UserContextId != this.Id)
        {
            return;
        }

        if (this.pages.Any(p => p.Id == args.BrowsingContextId))
        {
            return;
        }

        this.pages.Add(new Page(this.driver, args.BrowsingContextId, this.inspector));
    }

    private void OnContextDestroyed(BrowsingContextEventArgs args)
    {
        Page? page = this.pages.FirstOrDefault(p => p.Id == args.BrowsingContextId);
        if (page is not null)
        {
            this.pages.Remove(page);
        }
    }
}
