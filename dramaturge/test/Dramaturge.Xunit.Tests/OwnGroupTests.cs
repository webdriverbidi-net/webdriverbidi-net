// <copyright file="OwnGroupTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Xunit;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using Dramaturge.TestUtilities;

public class OwnGroupTests(FakeBrowserFixture fixture) : PageTest
{
    private const string SessionName = "own-group";

    protected override BrowserOptions? BrowserOptions => new() { AcceptInsecureCerts = true };

    [Fact]
    public async Task ClassThatConfiguresTheLauncherHasItsOwnGroup()
    {
        SharedTest shared = new();
        await shared.InitializeAsync();
        try
        {
            Assert.NotSame(shared.Group, this.Group);
            Assert.Contains(this.Browser.Id, fixture.Server.SessionFor(SessionName).UserContextIds);
        }
        finally
        {
            await shared.DisposeAsync();
        }
    }

    [Fact]
    public async Task BrowsersUseTheClassBrowserOptionsUnlessGivenTheirOwn()
    {
        await this.NewBrowserAsync(new BrowserOptions() { AcceptInsecureCerts = false });

        IReadOnlyList<JsonObject> commands = fixture.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("browser.createUserContext");
        Assert.True((bool)commands[^2]["params"]!["acceptInsecureCerts"]!);
        Assert.False((bool)commands[^1]["params"]!["acceptInsecureCerts"]!);
    }

    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return fixture.ConnectTo(SessionName);
    }

    private sealed class SharedTest : BrowserTest
    {
    }
}
