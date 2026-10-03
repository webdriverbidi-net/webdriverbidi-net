// <copyright file="OwnGroupTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TUnit;

using System.Text.Json.Nodes;
using Dramaturge.Browsers;
using global::TUnit.Core;

// The tests read the commands of the class's session in order.
[NotInParallel(nameof(OwnGroupTests))]
public class OwnGroupTests : PageTest
{
    private const string SessionName = "own-group";

    protected override BrowserOptions? BrowserOptions => new() { AcceptInsecureCerts = true };

    [Test]
    public async Task ClassThatConfiguresTheLauncherHasItsOwnGroup()
    {
        SharedTest shared = new();
        await shared.SetUpBrowsersAsync();
        try
        {
            await Assert.That(this.Group).IsNotSameReferenceAs(shared.Group);
            await Assert.That(FakeBrowserSetUp.Server.SessionFor(SessionName).UserContextIds).Contains(this.Browser.Id);
        }
        finally
        {
            await shared.TearDownBrowsersAsync();
        }
    }

    [Test]
    public async Task BrowsersUseTheClassBrowserOptionsUnlessGivenTheirOwn()
    {
        await this.NewBrowserAsync(new BrowserOptions() { AcceptInsecureCerts = false });

        IReadOnlyList<JsonObject> commands = FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("browser.createUserContext");
        await Assert.That((bool)commands[^2]["params"]!["acceptInsecureCerts"]!).IsTrue();
        await Assert.That((bool)commands[^1]["params"]!["acceptInsecureCerts"]!).IsFalse();
    }

    protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
    {
        return FakeBrowserSetUp.ConnectTo(SessionName);
    }

    private sealed class SharedTest : BrowserTest
    {
    }
}
