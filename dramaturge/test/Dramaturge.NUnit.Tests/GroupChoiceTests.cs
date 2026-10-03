// <copyright file="GroupChoiceTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.NUnit;

using Dramaturge.Browsers;

// The tests here create test objects themselves, which share the group of this fixture, as its tests would.
public class GroupChoiceTests
{
    private const string SessionName = "group-choice";
    private static readonly DramaturgeOptions Options = new();

    [OneTimeTearDown]
    public Task CloseGroupAsync() => BrowserTest.CloseFixtureGroupAsync();

    [Test]
    public async Task FixtureThatSetsOnlyGroupOptionsHasItsOwnGroupThatClosesWithTheFixture()
    {
        // The first test to need the fixture's group launches it, with its class's launcher and options.
        LauncherTest launcherTest = new();
        await launcherTest.SetUpBrowsersAsync();
        OptionsTest optionsTest = new();
        await optionsTest.SetUpBrowsersAsync();
        SharedTest sharedTest = new();
        await sharedTest.SetUpBrowsersAsync();

        Assert.That(optionsTest.Group, Is.SameAs(launcherTest.Group));
        Assert.That(optionsTest.Group.Options, Is.SameAs(Options));
        Assert.That(optionsTest.Group, Is.Not.SameAs(sharedTest.Group));
        await launcherTest.TearDownBrowsersAsync();
        await optionsTest.TearDownBrowsersAsync();
        await sharedTest.TearDownBrowsersAsync();

        await BrowserTest.CloseFixtureGroupAsync();

        Assert.That(FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("session.end"), Has.Count.EqualTo(1));
    }

    private sealed class LauncherTest : BrowserTest
    {
        protected override DramaturgeOptions? GroupOptions => Options;

        protected override BrowserLauncherBuilder ConfigureLauncher(BrowserLauncherBuilder builder)
        {
            return FakeBrowserSetUp.ConnectTo(SessionName);
        }
    }

    private sealed class OptionsTest : BrowserTest
    {
        protected override DramaturgeOptions? GroupOptions => new();
    }

    private sealed class SharedTest : BrowserTest
    {
    }
}
