// <copyright file="OwnGroupTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;

[TestClass]
public class OwnGroupTests : PageTest
{
    private const string SessionName = "own-group";

    protected override BrowserOptions? BrowserOptions => new() { AcceptInsecureCerts = true };

    [TestMethod]
    public async Task ClassThatConfiguresTheLauncherHasItsOwnGroup()
    {
        SharedTest shared = new() { TestContext = this.TestContext };
        await shared.SetUpBrowsersAsync();
        try
        {
            Assert.AreNotSame(shared.Group, this.Group);
            Assert.Contains(this.Browser.Id, FakeBrowserSetUp.Server.SessionFor(SessionName).UserContextIds);
        }
        finally
        {
            await shared.TearDownBrowsersAsync();
        }
    }

    [TestMethod]
    public async Task BrowsersUseTheClassBrowserOptionsUnlessGivenTheirOwn()
    {
        await this.NewBrowserAsync(new BrowserOptions() { AcceptInsecureCerts = false });

        IReadOnlyList<JsonObject> commands = FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("browser.createUserContext");
        Assert.IsTrue((bool)commands[^2]["params"]!["acceptInsecureCerts"]!);
        Assert.IsFalse((bool)commands[^1]["params"]!["acceptInsecureCerts"]!);
    }

    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return FakeBrowserSetUp.ConnectTo(SessionName);
    }

    private sealed class SharedTest : BrowserTest
    {
    }
}
