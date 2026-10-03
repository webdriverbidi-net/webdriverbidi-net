// <copyright file="PageTest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using global::Xunit;

/// <summary>
/// A base class for tests that each use an isolated browser and a page in it, opened before the test and closed after it.
/// </summary>
public abstract class PageTest : BrowserTest
{
    private Browser? browser;
    private Page? page;

    /// <summary>
    /// Gets the test's browser, opened with <see cref="BrowserTest.BrowserOptions"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown before the test starts.</exception>
    public Browser Browser => this.browser ?? throw new InvalidOperationException("The browser is available once the test has started.");

    /// <summary>
    /// Gets the test's page.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown before the test starts.</exception>
    public Page Page => this.page ?? throw new InvalidOperationException("The page is available once the test has started.");

    /// <summary>
    /// Opens the test's browser and page. A derived class that overrides this must call it.
    /// </summary>
    /// <returns>A task that completes when the page is open.</returns>
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);
        this.browser = await this.NewBrowserAsync().ConfigureAwait(false);
        this.page = await this.browser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false);
    }
}
