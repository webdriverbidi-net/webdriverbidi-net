// <copyright file="PageTest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// A base class for test classes whose tests each use an isolated browser and a page in it, opened before the test
/// and closed after it.
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
    /// Opens the test's browser and page, after <see cref="BrowserTest.SetUpBrowsersAsync"/>.
    /// </summary>
    /// <returns>A task that completes when the page is open.</returns>
    [TestInitialize]
    public async Task OpenPageAsync()
    {
        this.browser = await this.NewBrowserAsync().ConfigureAwait(false);
        this.page = await this.browser.NewPageAsync(cancellationToken: this.TestContext.CancellationTokenSource.Token).ConfigureAwait(false);
    }
}
