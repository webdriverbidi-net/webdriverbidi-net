// <copyright file="CloseFailureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using Dramaturge.Browsers;

// The class's session is left unable to close browsers, so it has a session of its own.
public class CloseFailureTests(FakeBrowserFixture fixture) : IClassFixture<ClassBrowserGroup>
{
    private const string SessionName = "close-failure";

    [Fact]
    public async Task FailureToCloseABrowserIsAWarningAndTheOthersStillClose()
    {
        ClosingTest test = new(fixture);
        await test.InitializeAsync();
        Browser second = await test.NewBrowserAsync();
        fixture.Server.SessionFor(SessionName).RemoteEnd.FailWith("browser.removeUserContext", "unknown error", "The browser cannot be closed.");

        await test.DisposeAsync();

        Assert.Equal(2, fixture.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("browser.removeUserContext").Count);
        Assert.All(TestContext.Current.Warnings ?? [], warning => Assert.StartsWith("Dramaturge could not close a browser: ", warning));
        Assert.Equal(2, TestContext.Current.Warnings?.Count);
        Assert.NotNull(second);
    }

    private sealed class ClosingTest(FakeBrowserFixture fixture) : PageTest
    {
        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return fixture.ConnectTo(SessionName);
        }
    }
}
