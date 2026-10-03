// <copyright file="CloseFailureTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.NUnit;

using global::NUnit.Framework.Interfaces;

using Dramaturge.Browsers;

// The fixture's session is left unable to close browsers, so it has a session of its own.
public class CloseFailureTests
{
    private const string SessionName = "close-failure";

    [OneTimeTearDown]
    public Task CloseGroupAsync() => BrowserTest.CloseFixtureGroupAsync();

    [Test]
    public async Task FailureToCloseABrowserIsWrittenToTheOutputAndTheOthersStillClose()
    {
        ClosingTest test = new();
        await test.SetUpBrowsersAsync();
        await test.NewBrowserAsync();
        FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.FailWith("browser.removeUserContext", "unknown error", "The browser cannot be closed.");

        TestEnding ending = await TestEnding.EndAsync(test, ResultState.Success);

        Assert.That(FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("browser.removeUserContext"), Has.Count.EqualTo(2));
        string[] lines = ending.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.That(lines, Has.Length.EqualTo(2).And.All.StartWith("Dramaturge could not close a browser: "));
    }

    private sealed class ClosingTest : PageTest
    {
        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return FakeBrowserSetUp.ConnectTo(SessionName);
        }
    }
}
