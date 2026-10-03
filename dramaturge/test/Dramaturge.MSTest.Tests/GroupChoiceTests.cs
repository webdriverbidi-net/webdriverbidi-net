// <copyright file="GroupChoiceTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.MSTest;

using Dramaturge.Browsers;

// The tests here create test objects themselves, which share the group of this class, as its tests would.
[TestClass]
public class GroupChoiceTests
{
    private const string SessionName = "group-choice";
    private static readonly DramaturgeOptions Options = new();

    public TestContext TestContext { get; set; } = null!;

    [ClassCleanup]
    public static Task CloseGroupAsync(TestContext context) => BrowserTest.CloseClassGroupAsync(context);

    [TestMethod]
    public async Task ClassThatSetsOnlyGroupOptionsHasItsOwnGroupThatClosesWithTheClass()
    {
        // The first test to need the class's group launches it, with its class's launcher and options.
        LauncherTest launcherTest = new() { TestContext = this.TestContext };
        await launcherTest.SetUpBrowsersAsync();
        OptionsTest optionsTest = new() { TestContext = this.TestContext };
        await optionsTest.SetUpBrowsersAsync();
        SharedTest sharedTest = new() { TestContext = this.TestContext };
        await sharedTest.SetUpBrowsersAsync();

        Assert.AreSame(launcherTest.Group, optionsTest.Group);
        Assert.AreSame(Options, optionsTest.Group.Options);
        Assert.AreNotSame(sharedTest.Group, optionsTest.Group);
        await launcherTest.TearDownBrowsersAsync();
        await optionsTest.TearDownBrowsersAsync();
        await sharedTest.TearDownBrowsersAsync();

        await BrowserTest.CloseClassGroupAsync(this.TestContext);

        Assert.HasCount(1, FakeBrowserSetUp.Server.SessionFor(SessionName).RemoteEnd.CommandsFor("session.end"));
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
