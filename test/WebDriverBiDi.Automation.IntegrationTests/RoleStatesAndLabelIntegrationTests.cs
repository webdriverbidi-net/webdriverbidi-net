// <copyright file="RoleStatesAndLabelIntegrationTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Automation.TestUtilities;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;

public class RoleStatesAndLabelIntegrationTests
{
    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task RoleStatesFilterTheBrowsersRoleMatches(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("states.html"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3, await page.GetByRole("checkbox").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("checkbox", "Checked box", new RoleStates() { Checked = ToggleState.On }).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("checkbox", states: new RoleStates() { Checked = ToggleState.Off }).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("checkbox", states: new RoleStates() { Checked = ToggleState.Mixed }).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("button", states: new RoleStates() { Pressed = ToggleState.On }).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("button", states: new RoleStates() { Expanded = true }).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("button", states: new RoleStates() { Disabled = true }).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("tab", states: new RoleStates() { Selected = true }).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByRole("heading", states: new RoleStates() { Level = 2 }).CountAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(TestBrowsers.All), MemberType = typeof(TestBrowsers))]
    public async Task LabelsFindTheirControls(BrowserKind browserKind)
    {
        await using TestPageServer server = await TestPageServer.StartAsync();
        await using BrowserGroup group = await TestBrowsers.LaunchAsync(browserKind);
        Page page = await group.DefaultBrowser.NewPageAsync(cancellationToken: TestContext.Current.CancellationToken);
        await page.NavigateAsync(server.UrlFor("states.html"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(1, await page.GetByLabel("email").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByLabel("Email address", exact: true).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(0, await page.GetByLabel("email address", exact: true).CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByLabel("password").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByLabel("search the site").CountAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, await page.GetByLabel("close dialog").CountAsync(TestContext.Current.CancellationToken));
        Assert.True(await page.GetByLabel("close dialog").IsVisibleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, await page.Locate(new CssLocator("#account")).GetByLabel("s").CountAsync(TestContext.Current.CancellationToken));
    }
}
