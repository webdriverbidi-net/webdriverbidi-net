// <copyright file="OwnGroupTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.NUnit;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;

public class OwnGroupTests : PageTest
{
    private const string SessionName = "own-group";

    protected override BrowserOptions? BrowserOptions => new() { AcceptInsecureCerts = true };

    [Test]
    public async Task FixtureThatConfiguresTheLauncherHasItsOwnGroup()
    {
        SharedTest shared = new();
        await shared.SetUpBrowsersAsync();
        try
        {
            Assert.That(this.Group, Is.Not.SameAs(shared.Group));
            Assert.That(FakeBrowserSetUp.Server.SessionFor(SessionName).UserContextIds, Does.Contain(this.Browser.Id));
        }
        finally
        {
            await shared.TearDownBrowsersAsync();
        }
    }

    [Test]
    public async Task BrowsersUseTheFixtureBrowserOptionsUnlessGivenTheirOwn()
    {
        await this.NewBrowserAsync(new BrowserOptions() { AcceptInsecureCerts = false });

        IReadOnlyList<JsonObject> commands = FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("browser.createUserContext");
        Assert.That((bool)commands[^2]["params"]!["acceptInsecureCerts"]!, Is.True);
        Assert.That((bool)commands[^1]["params"]!["acceptInsecureCerts"]!, Is.False);
    }

    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return FakeBrowserSetUp.ConnectTo(SessionName);
    }

    private sealed class SharedTest : BrowserTest
    {
    }
}
