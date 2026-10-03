// <copyright file="TestFrameworksXunitSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/test-frameworks.md and src/Dramaturge.Xunit/README.md. The namespace is outside
// Dramaturge's, as a test project's is, so that "Xunit" means xUnit's namespace.

// An assembly attribute comes before the namespace, so the usings do too.
using Dramaturge;
using Dramaturge.Browsers;
using Dramaturge.Xunit;
using WebDriverBiDi.BrowsingContext;
using Xunit;
using static Dramaturge.Assertions;

#region XunitRegistration
[assembly: AssemblyFixture(typeof(Dramaturge.Xunit.DramaturgeAssemblyFixture))]
#endregion

namespace TestFrameworkSamples.XunitTests;

#region XunitPageTest
public class SignInTests : PageTest
{
    [Fact]
    public async Task SignsIn()
    {
        await this.Page.NavigateAsync("https://example.com/sign-in");
        await this.Page.GetByLabel("Email").FillAsync("someone@example.com");
        await this.Page.GetByRole("button", "Sign in").ClickAsync();
        await Expect(this.Page.GetByRole("heading", "Welcome")).ToBeVisibleAsync();
    }
}
#endregion

#region SeveralBrowsers
public class ChatTests : BrowserTest
{
    [Fact]
    public async Task MessageReachesTheOtherUser()
    {
        // Each browser has its own cookies and storage, so each signs in as a different user.
        Page alice = await (await this.NewBrowserAsync()).NewPageAsync();
        Page bob = await (await this.NewBrowserAsync()).NewPageAsync();
        await alice.NavigateAsync("https://example.com/chat?user=alice");
        await bob.NavigateAsync("https://example.com/chat?user=bob");

        await alice.GetByLabel("Message").FillAsync("Hello, Bob");
        await alice.GetByRole("button", "Send").ClickAsync();

        await Expect(bob.GetByRole("log")).ToContainTextAsync("Hello, Bob");
    }
}
#endregion

#region ClassSettings
public class MobileLayoutTests : PageTest
{
    // Applies to the browser of each test of this class.
    protected override BrowserOptions? BrowserOptions => new() { Viewport = new Viewport() { Width = 390, Height = 844 }, Locale = "en-GB" };

    // Overriding the launcher or the group's options gives the class a browser of its own, launched for its tests.
    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return builder.WithLaunchTimeout(TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task MenuIsCollapsed()
    {
        await this.Page.NavigateAsync("https://example.com");
        await Expect(this.Page.GetByRole("button", "Menu")).ToBeVisibleAsync();
    }
}
#endregion

#region SharedSettings
// Registered in place of DramaturgeAssemblyFixture:
// [assembly: AssemblyFixture(typeof(ShopFixture))]
public class ShopFixture : DramaturgeAssemblyFixture
{
    protected override DramaturgeOptions? GroupOptions => new() { ActionTimeout = TimeSpan.FromSeconds(10), TestIdAttribute = "data-test" };

    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        string? grid = Environment.GetEnvironmentVariable("SELENIUM_GRID_URL");
        return grid is null ? builder : BrowserLauncher.Configure(BrowserKind.Chrome).LaunchUsingRemoteGrid(new Uri(grid));
    }
}
#endregion

#region Screenshots
public class ReportTests : PageTest
{
    public ReportTests()
    {
        this.ArtifactsDirectory = Path.Combine(AppContext.BaseDirectory, "screenshots");
    }

    [Fact]
    public async Task PrintsWithoutScreenshots()
    {
        // This test's failures are not captured.
        this.ScreenshotOnFailure = false;
        await this.Page.NavigateAsync("https://example.com/report");
        await Expect(this.Page.GetByRole("button", "Print")).ToBeEnabledAsync();
    }
}
#endregion
