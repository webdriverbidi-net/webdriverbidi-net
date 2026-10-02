// <copyright file="SkillFixtureSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Docs.Code.Skill;

using Dramaturge;
using Dramaturge.Browsers;

#region Fixture
/// <summary>
/// Launches one browser for a test class's tests, and closes it after them.
/// </summary>
public sealed class BrowserFixture : IAsyncDisposable
{
    private BrowserGroup? group;

    /// <summary>
    /// Gets the launched browser.
    /// </summary>
    public BrowserGroup Group => this.group ?? throw new InvalidOperationException("The browser has not been launched.");

    /// <summary>
    /// Launches the browser.
    /// </summary>
    /// <returns>A task that completes when the browser is launched.</returns>
    public async Task InitializeAsync()
    {
        this.group = await BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Chrome).WithHeadlessOption());
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (this.group is not null)
        {
            await this.group.DisposeAsync();
        }
    }
}

/// <summary>
/// The base of a test class: each test gets a browser of its own, isolated from the others, and a page in it.
/// </summary>
/// <param name="fixture">The launched browser, shared by the class's tests.</param>
public abstract class PageTest(BrowserFixture fixture) : IAsyncDisposable
{
    private Browser? browser;

    /// <summary>
    /// Gets the test's page.
    /// </summary>
    protected Page Page { get; private set; } = null!;

    /// <summary>
    /// Opens the test's browser and page.
    /// </summary>
    /// <returns>A task that completes when the page is open.</returns>
    public async Task InitializeAsync()
    {
        this.browser = await fixture.Group.CreateBrowserAsync();
        this.Page = await this.browser.NewPageAsync();
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
#endregion
