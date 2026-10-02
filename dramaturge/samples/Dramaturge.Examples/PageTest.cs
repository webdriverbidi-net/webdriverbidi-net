// <copyright file="PageTest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Examples;

/// <summary>
/// The base of a test class: each test gets a browser of its own, so that no cookie or storage carries over from
/// another test, and a page in it with the shop served.
/// </summary>
/// <param name="fixture">The launched browser, shared by the class's tests.</param>
public abstract class PageTest(BrowserFixture fixture) : IClassFixture<BrowserFixture>, IAsyncLifetime
{
    private Browser? browser;
    private Page? page;

    /// <summary>
    /// Gets the test's page.
    /// </summary>
    protected Page Page => this.page ?? throw new InvalidOperationException("The page has not been opened.");

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        this.browser = await fixture.Group.CreateBrowserAsync();
        this.page = await this.browser.NewPageAsync();
        await Shop.ServeAsync(this.page);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (this.browser is not null)
        {
            await this.browser.CloseAsync();
        }
    }
}
